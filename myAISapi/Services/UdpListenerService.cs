using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
		//private readonly IUdpMessageStore _messageStore;
		private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>();

		private const int UdpPort = 60100;

		public UdpListenerService(ILogger<UdpListenerService> logger, IUdpMessageStore messageStore)
		{
			_logger = logger;
			_udpClient = new UdpClient("ais-iot.pro.vn", 60100);
		}


		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"✅ UDP Listener started on port {UdpPort}.");

			// Gửi ping lần đầu
			byte[] pingMsg = Encoding.ASCII.GetBytes("a");
			await _udpClient.SendAsync(pingMsg, pingMsg.Length);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var receiveTask = _udpClient.ReceiveAsync();
					var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

					var completed = await Task.WhenAny(receiveTask, timeoutTask);

					if (completed == receiveTask)
					{
						var result = receiveTask.Result;
						string message = Encoding.UTF8.GetString(result.Buffer);

						_messageQueue.Add(message, stoppingToken);
					}
					else
					{
						if (stoppingToken.IsCancellationRequested)
							break;

						_logger.LogWarning("⏳ No UDP data received within timeout. Sending ping...");
						await _udpClient.SendAsync(pingMsg, pingMsg.Length);
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "❌ Error receiving UDP data.");
					await Task.Delay(1000, stoppingToken);
				}
			}

			_logger.LogInformation("❎ UDP Listener stopped.");
		}


		public override void Dispose()
		{
			_udpClient?.Dispose();
			_messageQueue?.Dispose();
			base.Dispose();
		}

		public BlockingCollection<string> GetMessageQueue()
		{
			return _messageQueue;
		}
	}
}
