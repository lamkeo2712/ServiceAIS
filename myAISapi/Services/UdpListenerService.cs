using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using myAISapi.Controllers;
using myAISapi.Decoder;
using myAISapi.Models;

namespace myAISapi.Services
{
	public class UdpListenerService : BackgroundService
	{
		private readonly ILogger<UdpListenerService> _logger;
		private readonly UdpClient _udpClient;
		private readonly IUdpMessageStore _messageStore;

		private const int UdpPort = 60100;  // Cổng nhận dữ liệu UDP

		public UdpListenerService(ILogger<UdpListenerService> logger, IUdpMessageStore messageStore)
		{
			_logger = logger;
			_messageStore = messageStore;
			_udpClient = new UdpClient("ais-iot.pro.vn", 60100);
		}


		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"✅ UDP Listener started on port {UdpPort}.");

			// Ping
			byte[] msg = Encoding.ASCII.GetBytes("a");
			await _udpClient.SendAsync(msg, msg.Length); 
			_logger.LogInformation($"✅ UDP message sent to \"ais-iot.pro.vn\":{UdpPort} : {Encoding.ASCII.GetString(msg)} ({msg.Length} bytes).");

			_logger.LogInformation($"✅ Check:{stoppingToken.IsCancellationRequested}");
			int i = 0;
			while (!stoppingToken.IsCancellationRequested)
			//while (i < 3)
			{
				try
				{
					var result = await _udpClient.ReceiveAsync();
					string message = Encoding.UTF8.GetString(result.Buffer);
					_messageStore.AddMessage(message);
					//_logger.LogInformation($"📩 All message: {message}");
					i++;
				}
				catch (Exception ex)
				{
					_logger.LogError($"❌ Error receiving UDP data: {ex.Message}");
				}
			}

			_logger.LogInformation("❎ UDP Listener stopped.");
		}


		public override void Dispose()
		{
			_udpClient?.Dispose();
			base.Dispose();
		}
	}
}
