using System.ComponentModel.DataAnnotations;

namespace myAISapi.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string PasswordHash { get; set; }

		[Required]
		public string Role { get; set; }
		public string? HoTen { get; set; }
		public string? Email { get; set; }
		public string? DienThoai { get; set; }
		public string? RefreshToken { get; set; }
		public DateTime? RefreshTokenExpiryTime { get; set; }
		public string? PlanType { get; set; } = "Free";
		public DateTime? PlanExpiredAt { get; set; }
	}

}
