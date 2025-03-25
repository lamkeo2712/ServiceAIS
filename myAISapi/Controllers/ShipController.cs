using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myAISapi.Data;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myAISapi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ShipController : ControllerBase
	{
		private readonly AppDBContext _context;

		public ShipController(AppDBContext context)
		{
			_context = context;
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Data/DoRequest")]
		public async Task<object> DoRequest([FromBody] RequestModel request)
		{
			if (request == null || string.IsNullOrEmpty(request.ProcedureName))
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				request.ProcedureName,
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}
	}

	public class RequestModel
	{
		public string ProcedureName { get; set; }
		public string ThamSo { get; set; }
	}
}
