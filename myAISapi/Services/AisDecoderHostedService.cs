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
												if (tau is not string )
												{
													if (IsTauValid((DM_Tau)tau))
													{
														_shipStore.AddShip((DM_Tau)tau);
													}
												}
												if (hanhtrinh is not string) {
													if (IsHanhTrinhValid((DM_HanhTrinh)hanhtrinh))
													{
														_routeStore.AddRoute((DM_HanhTrinh)hanhtrinh);
														//_logger.LogInformation(JsonSerializer.Serialize((DM_HanhTrinh)hanhtrinh));
													}
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
