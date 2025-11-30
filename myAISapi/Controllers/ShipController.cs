using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myAISapi.Data;
using myAISapi.Models;
using myAISapi.Services;
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
		private readonly IAlertService _alertService;

		public ShipController(AppDBContext context, ICassandraHanhTrinhRepository repo, IAlertService alertService)
		{
			_context = context;
			_repo = repo;
			_alertService = alertService;
		}

		//[HttpPost]
		//[Route("Data/DoRequest")]
		//public async Task<object> DoRequest([FromBody] RequestModel request)
		//{
		//	if (request == null)
		//	{
		//		return BadRequest("Invalid request.");
		//	}

		//	// Gọi phương thức từ instance của AppDBContext
		//	return await _context.ExecuteProcedureAsync(
		//		request.ProcedureName,
		//		request.ThamSo,
		//		HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
		//	);
		//}

		[HttpPost]
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

		[HttpPost]
		[Route("Data/GetTau")]
		public async Task<object> GetTau([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_Search2",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Route("Data/GetShipType")]
		public async Task<object> GetShipType([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			return await _context.ExecuteProcedureAsync(
				"Proc_DM_ShipType_Search",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/GetTauTD")]
		public async Task<object> GetTauTD([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_TD_Search",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/UpdateTauTD")]
		public async Task<object> UpdateTauTD([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_TD_Update",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/DeleteTauTD")]
		public async Task<object> DeleteTauTD([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_TD_Delete",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/GetVung")]
		public async Task<object> GetVung([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Vung_Search",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/UpdateVung")]
		public async Task<object> UpdateVung([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Vung_Update",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/DeleteVung")]
		public async Task<object> DeleteVung([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Vung_Delete",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost]
		[Authorize(Policy = "PaidUserOnly")]
		[Route("Data/GetTauInPolygon")]
		public async Task<object> GetTauInPolygon([FromBody] RequestModel request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			// Gọi phương thức từ instance của AppDBContext
			return await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_Polygon_Search",
				request.ThamSo,
				HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "test"
			);
		}

		[HttpPost("broadcast")]
		public async Task<IActionResult> BroadcastTestAlert()
		{
			var alert = new
			{
				Type = "Test",
				Message = "Có thông báo test từ server",
				Time = DateTime.UtcNow
			};

			await _alertService.PushBroadcastAsync(alert);
			return Ok("Đã gửi alert");
		}

		[HttpPost("user/{userId}")]
		public async Task<IActionResult> AlertUser(string userId)
		{
			var alert = new
			{
				Type = "UserAlert",
				Message = "Thông báo riêng cho user " + userId,
				Time = DateTime.UtcNow
			};

			await _alertService.PushToUserAsync(userId, alert);
			return Ok("Đã gửi alert cho user");
		}
	}


	

	public class RequestModel
	{
		public string ThamSo { get; set; }
	}
	public class GetHanhTrinhRequest
	{
		public int MMSI { get; set; }
		public int Hours { get; set; }
	}
}
