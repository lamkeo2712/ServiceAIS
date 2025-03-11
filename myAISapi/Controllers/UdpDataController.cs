using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using myAISapi.Models;
using myAISapi.Services;
using System.Collections.Concurrent;

namespace myAISapi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UdpDataController : ControllerBase
	{
		private readonly ILogger<AisDecoderHostedService> _logger;
		private readonly IUdpMessageStore _messageStore;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_Tau_HS_Store _shipHsStore;
		private readonly IDM_HanhTrinh_Store _routeStore;
		private readonly IServiceScopeFactory _scopeFactory;

		public UdpDataController(
			IUdpMessageStore messageStore,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_Tau_HS_Store shipHsStore,
			IDM_HanhTrinh_Store routeStore)
		{
			_messageStore = messageStore;
			_decodedAISStore = decodedAISStore;
			_scopeFactory = scopeFactory;
			_shipStore = shipStore;
			_shipHsStore = shipHsStore;
			_routeStore = routeStore;
		}

		[HttpGet("messages")]
		public IActionResult GetMessages()
		{
			return Ok(_messageStore.GetAllMessages());
		}
		[HttpGet("hs")]
		public IActionResult GetHS()
		{
			return Ok(_shipHsStore.GetAllShip());
		}
		[HttpGet("tau")]
		public IActionResult GetShip()
		{
			return Ok(_shipStore.GetAllShip());
		}
		[HttpGet("hanhtrinh")]
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
