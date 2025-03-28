using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using myAISapi.Models;
using myAISapi.Services;
using System.Collections.Concurrent;

namespace myAISapi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UdpDataController : ControllerBase
	{
		private readonly IUdpMessageStore _messageStore;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_HanhTrinh_Store _routeStore;
		private readonly IServiceScopeFactory _scopeFactory;

		public UdpDataController(
			IUdpMessageStore messageStore,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_HanhTrinh_Store routeStore)
		{
			_messageStore = messageStore;
			_decodedAISStore = decodedAISStore;
			_scopeFactory = scopeFactory;
			_shipStore = shipStore;
			_routeStore = routeStore;
		}

		[HttpGet("messages")]
		[Authorize(Policy = "AdminOnly")]
		public IActionResult GetMessages()
		{
			return Ok(_messageStore.GetAllMessages());
		}

		[HttpGet("tau")]
		[Authorize(Policy = "AdminOnly")]
		public IActionResult GetShip()
		{
			return Ok(_shipStore.GetAllShip());
		}
		[HttpGet("hanhtrinh")]
		[Authorize(Policy = "Admin&Guest")]
		public IActionResult GetRoute()
		{
			return Ok(_routeStore.GetAllRoute());
		}

		[HttpDelete("clear")]
		public IActionResult ClearMessages()
		{
			_messageStore.ClearMessages();
			return Ok("🗑️ Messages cleared.");
		}
	}
}
