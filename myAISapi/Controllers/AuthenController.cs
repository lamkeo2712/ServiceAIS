using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using myAISapi.Models.RequestModels;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using myAISapi.Data;
using myAISapi.Models;
using System.Linq;
using myAISapi.Decoder;
using Microsoft.AspNetCore.Identity;

namespace myAISapi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenController : ControllerBase
	{
		private readonly AppDBContext _context; // Inject DbContext

		public AuthenController(AppDBContext context) // Thêm DbContext vào constructor
		{
			_context = context;
		}

		[HttpPost("signin")]
		public IActionResult Signin([FromBody] UserRequestModel user)
		{
			// Xác thực người dùng
			string hashedPassword = PasswordHasher.HashPassword(user.Password);

			_context.Users.Add(new User
			{
				Username = user.Username,
				PasswordHash = hashedPassword,
				Role = "Guest",
			});

			try
			{
				_context.SaveChanges(); 
				return Ok($"đã thêm tài khoản {user.Username} với role Guest");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi thêm tài khoản: {ex.Message}");
			}
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] UserRequestModel userRequestModel)
		{
			// Xác thực người dùng
			var user = AuthenticateUser(userRequestModel.Username, userRequestModel.Password);
			if (user == null)
			{
				return Unauthorized();
			}
			// Tạo claims
			var claims = new[] {
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Role, user.Role), // Lấy role từ database
            };
			// Tạo JWT token
			var accessToken = GenerateJwtToken(claims);
			var refreshToken = GenerateRefreshToken();
			// Lưu refresh token vào database
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			_context.SaveChanges();

			return Ok(new { accessToken, refreshToken });
		}
		private User AuthenticateUser(string username, string password)
		{
			// Tìm người dùng trong database
			var user = _context.Users.FirstOrDefault(u => u.Username == username);

			// Kiểm tra xem người dùng có tồn tại không
			if (user == null || user.Username == null)
			{
				return null;
			}

			// TODO: Kiểm tra password (sử dụng hash và salt)
			if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
			{
				return null;
			}

			return user;
		}

		[HttpPost("refresh")]
		public IActionResult RefreshToken(string refreshToken)
		{
			var user = _context.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

			if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
			{
				return BadRequest("Invalid refresh token");
			}

			// Tạo access token mới
			var claims = new[] {
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Role, user.Role),
			};
			var newAccessToken = GenerateJwtToken(claims);

			return Ok(new { accessToken = newAccessToken });
		}

		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[64];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}

		private string GenerateJwtToken(Claim[] claims)
		{
			var privateKey = System.IO.File.ReadAllText("./Keys/private.key");
			var rsa = RSA.Create();
			rsa.ImportFromPem(privateKey);

			var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(1), // Thời gian hết hạn
				SigningCredentials = signingCredentials,
				Issuer = "http://localhost:5013", // Thay bằng issuer của bạn
				Audience = "http://localhost:3030" // Thay bằng audience của bạn
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		[HttpGet("forbidden")]
		public IActionResult GetForbidden()
		{
			return Forbid(); // Sử dụng ForbidResult
		}

		[HttpGet("GetUser")]
		public IActionResult GetUser()
		{
			var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
			var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

			var user = new
			{
				Username = username,
				Role = role
			};

			return Ok(user);
		}

	}
}
