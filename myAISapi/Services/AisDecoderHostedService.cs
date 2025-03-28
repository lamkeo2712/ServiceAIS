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
		//private readonly IUdpMessageStore _messageStore;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_HanhTrinh_Store _routeStore;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly UdpListenerService _udpListenerService;


		public AisDecoderHostedService(
			ILogger<AisDecoderHostedService> logger,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_HanhTrinh_Store routeStore,
			UdpListenerService udpListenerService)
		{
			_decodedAISStore = decodedAISStore;
			_logger = logger;
			_scopeFactory = scopeFactory;
			_shipStore = shipStore;
			_routeStore = routeStore;
			_udpListenerService = udpListenerService;
		}

		// Thực hiện xử lý chạy nền tại đây
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("AIS Decoder Hosted Service is running.");

			var messageQueue = _udpListenerService.GetMessageQueue();//.GetConsumingEnumerable(stoppingToken);

			await Parallel.ForEachAsync(messageQueue.GetConsumingEnumerable(stoppingToken),
				new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, // Giới hạn số lượng task song song
				async (msg, ct) => // ct là CancellationToken
			//foreach (var msg in messageQueue)
			{
					try
					{
						//Console.WriteLine($"Batch: {msg}");
						string fullPayload = "";
						string[] splitMsg = msg.Split("\n");

						foreach (string s in splitMsg)
						{
							try
							{
								//Console.WriteLine($"S: {s}");

								dynamic result = MainDecode.AisDecode(s);
								if (result is not string)
								{
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
												//Console.WriteLine($"decoded: {JsonSerializer.Serialize(decodedData)}");
												_decodedAISStore.AddDecodedMessage((DecodedAISMessage)decodedData);
												await ProcessDecodedMessage((DecodedAISMessage)decodedData); // Chuyển sang async
												fullPayload = "";
											}
										}
										catch (Exception decodeEx)
										{
											_logger.LogError($"Error decoding payload: {decodeEx.Message}");
										}
									}
								}
							}
							catch (Exception decodeEx)
							{
								_logger.LogError($"Error in AisDecode: {decodeEx.Message}");
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogError($"Error in AIS decoder loop: {ex.Message}");
					}
				});

			_logger.LogInformation("AIS Decoder Hosted Service is stopping.");
		}

		private async Task ProcessDecodedMessage(DecodedAISMessage message)
		{
			//Sử dụng scope để lấy DbContext một cách chính xác
			using (var scope = _scopeFactory.CreateScope())
			{
				
				var shipStore = scope.ServiceProvider.GetRequiredService<IDM_Tau_Store>();
				var routeStore = scope.ServiceProvider.GetRequiredService<IDM_HanhTrinh_Store>();

				var tau = myAISapi.Data.DataHelper.Ship(message);
				var hanhtrinh = myAISapi.Data.DataHelper.Route(message);

				if (tau is DM_Tau validTau && IsTauValid(validTau))
				{
					shipStore.AddShip(validTau);
				}

				if (hanhtrinh is DM_HanhTrinh validRoute && IsHanhTrinhValid(validRoute))
				{
					routeStore.AddRoute(validRoute);
				}
			}
		}

		private bool IsTauValid(DM_Tau tau)
		{
			if(tau.VesselName == null && tau.IMONumber == 0 &&
				tau.CallSign == null && tau.ShipType == 0 &&
				tau.AISVersion == 0 && tau.TypeOfEPFD == 0 &&
				tau.DimensionToBow == 0 && tau.DimensionToStern == 0 &&
				tau.DimensionToPort == 0 && tau.DimensionToStar == 0 &&
				tau.ShipLength == 0 && tau.ShipWidth == 0 &&
				tau.Draught == 0 && tau.Destination == null &&
				tau.VirtualAidFlag == false && tau.OffPositionIndicator == false)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool IsHanhTrinhValid(DM_HanhTrinh ht)
		{
			if(ht.NavigationStatus == 0 && ht.RateOfTurn == 0 &&
				ht.SpeedOverGround == 0 && ht.PositionAccuracy == false &&
				ht.Longitude == 0 && ht.Latitude == 0 &&
				ht.CourseOverGround == 0 && ht.TrueHeading == 0 &&
				ht.DateTimeUTC == null && ht.ManeuverIndicator == 0 &&
				ht.RAIMFlags == false && ht.PositionFixType == 0 &&
				ht.StationType == 0 && ht.ReportInterval == 0 &&
				ht.DisplayFlag == false && ht.DSCFlag == false &&
				ht.ETADateTime == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}


	}
}
