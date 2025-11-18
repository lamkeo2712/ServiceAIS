using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myAISapi.Data;
using myAISapi.Models;
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
		private readonly ICassandraHanhTrinhRepository _repo;
		private readonly AppDBContext _context;

		public ShipController(AppDBContext context, ICassandraHanhTrinhRepository repo)
		{
			_context = context;
			_repo = repo;
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

		[HttpPost]
		[AllowAnonymous]
		[Route("Data/GetHanhTrinh")]
		public async Task<IActionResult> GetHanhTrinh([FromBody] GetHanhTrinhRequest request, CancellationToken ct)
		{
			if (request.Hours <= 0)
			{
				return BadRequest("Hours must be > 0.");
			}

			var data = await _repo.GetHanhTrinhAsync(request.MMSI, request.Hours, ct);

			return Ok(data);
		}
	}

	public class RequestModel
	{
		public string ProcedureName { get; set; }
		public string ThamSo { get; set; }
	}
	public class GetHanhTrinhRequest
	{
		public int MMSI { get; set; }
		public int Hours { get; set; }
	}
}
