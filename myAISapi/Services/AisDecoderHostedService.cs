using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using myAISapi.Decoder;
using myAISapi.Models;
using myAISapi.Data;
using System.Text.Json;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace myAISapi.Services
{
	// Kế thừa từ BackgroundService (Không cần IHostedService nữa)
	public class AisDecoderHostedService : BackgroundService
	{
		private readonly ILogger<AisDecoderHostedService> _logger;
		private readonly IUdpMessageStore _messageStore;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_Tau_HS_Store _shipHsStore;
		private readonly IDM_HanhTrinh_Store _routeStore;
		private readonly IServiceScopeFactory _scopeFactory;

		DataHelper dataHelper = new DataHelper();

		public AisDecoderHostedService(
			ILogger<AisDecoderHostedService> logger,
			IUdpMessageStore messageStore,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_Tau_HS_Store shipHsStore,
			IDM_HanhTrinh_Store routeStore)
		{
			_messageStore = messageStore;
			_decodedAISStore = decodedAISStore;
			_logger = logger;
			_scopeFactory = scopeFactory;
			_shipStore = shipStore;
			_shipHsStore = shipHsStore;
			_routeStore = routeStore;
		}

		// Thực hiện xử lý chạy nền tại đây
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("AIS Decoder Hosted Service is running.");

			while (!stoppingToken.IsCancellationRequested)
			{
				if (_messageStore.GetAllMessages().Length > 0)
				{
					try
					{
						string fullPayload = "";
						string msg = _messageStore.GetAllMessages()[0].Trim();
						string[] splitMsg = msg.Split("\n");

						foreach (string s in splitMsg)
						{
							try
							{
								dynamic result = MainDecode.AisDecode(s);
								//using (var scope = _scopeFactory.CreateScope())
								//{
								//	var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
								//	var ThamSoJSON = JsonSerializer.Serialize(new
								//	{
								//		raw_message = s,
								//		received_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
								//	});
								//	var exec = await dbContext.ExecuteProcedureAsync(
								//		"Proc_Rawdata_Update",
								//		ThamSoJSON,
								//		"Admin"
								//	);

								//	if (exec == 0)
								//	{
								//		Console.WriteLine("Không thể chèn dữ liệu AIS.");
								//	}
								//	else
								//	{
								//		Console.WriteLine("Dữ liệu AIS đã được lưu vào database.");
								//	}
								//}


								var messageCount = result.MessageCount;
								var fragNumber = result.FragNumber;
								var payload = result.Payload;

								fullPayload += payload;

								if (fragNumber == messageCount)
								{
									try
									{
										var decodedData = MainDecode.PayloadDecode(fullPayload);
										if (decodedData is not string)
										{
											_decodedAISStore.AddDecodedMessage((DecodedAISMessage)decodedData);
											try
											{
												var tau = dataHelper.Ship((DecodedAISMessage)decodedData);
												var hanhtrinh = dataHelper.Route((DecodedAISMessage)decodedData);
												//_logger.LogInformation(JsonSerializer.Serialize(tau));
												if (tau is not string)
												{
													_shipStore.AddShip((DM_Tau)tau);
													_shipHsStore.AddShip((DM_Tau)tau);
												}
												if (hanhtrinh is not string) { 
													_routeStore.AddRoute((DM_HanhTrinh)hanhtrinh);
													//_logger.LogInformation(JsonSerializer.Serialize((DM_HanhTrinh)hanhtrinh));
												}
												//else
												//{
												//	_logger.LogInformation($"loi: {hanhtrinh.ToString()} ");
												//}
											}
											catch (Exception Ex)
											{
												_logger.LogError($"Error parsing data: {Ex.Message}");
											}
										}
										//_logger.LogInformation(JsonSerializer.Serialize(decodedData));

										fullPayload = "";
									}
									catch (Exception decodeEx)
									{
										_logger.LogError($"Error decoding payload: {decodeEx.Message}");
									}
								}
							}
							catch (Exception decodeEx)
							{
								_logger.LogError($"Error in AisDecode: {decodeEx.Message}");
								continue; // Tiếp tục vòng lặp dù có lỗi
							}
						}

						// Chỉ chạy khi tất cả tin nhắn đã xử lý
						_messageStore.Delete(); // Xóa tin nhắn đầu tiên
						_logger.LogInformation($"đã xoá");
					}
					catch (Exception ex)
					{
						_logger.LogError($"Error in AIS decoder loop: {ex.Message}");
					}
				}

				await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
			}

			_logger.LogInformation("AIS Decoder Hosted Service is stopping.");
		}




	}
}
