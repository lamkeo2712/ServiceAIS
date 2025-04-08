using Microsoft.AspNetCore.Routing;
using myAISapi.Data;
using myAISapi.Decoder;
using myAISapi.Models;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace myAISapi.Services
{
	public class AisDBService : BackgroundService
	{
		private readonly ILogger<AisDBService> _logger;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_HanhTrinh_Store _routeStore;

		public AisDBService(
			//AisDecoderService aisDecoderService,
			ILogger<AisDBService> logger,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_HanhTrinh_Store routeStore)
		{
			_decodedAISStore = decodedAISStore;
			_logger = logger;
			_scopeFactory = scopeFactory;
			_shipStore = shipStore;
			_routeStore = routeStore;
		}

		// Thực hiện xử lý chạy nền tại đây
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("AIS Decoder Hosted Service is running.");

			var shipTask = ProcessShipsAsync(stoppingToken);
			var routeTask = ProcessRoutesAsync(stoppingToken);

			await Task.WhenAll
			(
				shipTask,
				routeTask
			);

			_logger.LogInformation("AIS DB Hosted Service is stopping.");
		}

		private async Task ProcessShipsAsync(CancellationToken stoppingToken)
		{
			const int BATCH_SIZE = 100;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var allShips = _shipStore.GetAllShip().ToList();

					for (int i = 0; i < allShips.Count; i += BATCH_SIZE)
					{
						var batch = allShips.Skip(i).Take(BATCH_SIZE).ToList();

						using (var scope = _scopeFactory.CreateScope())
						{
							var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
							await using var transaction = await dbContext.Database.BeginTransactionAsync();

							try
							{
								await BulkProcessShipsAsync(batch, dbContext);
								await AddShipHistoryAsync(batch, dbContext);
								await transaction.CommitAsync(); 
								Console.WriteLine($"Batch: {JsonSerializer.Serialize(batch)}");
								//Console.WriteLine("Du lieu tau đa đuoc luu vao database.");
								_shipStore.DeleteMessages(batch); // Xóa batch đã xử lý
							}
							catch
							{

								_logger.LogError($"Error Tau");
								await transaction.RollbackAsync();
								throw;
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in Ship processing: {ex.Message}");
				}

			}
		}

		private async Task BulkProcessShipsAsync(List<DM_Tau> ships, AppDBContext context)
		{
			var mmsiList = ships.Select(s => s.MMSI).ToList();

			// Lấy danh sách MMSI đã tồn tại
			var existingMmsis = await context.DM_Tau
				.Where(t => mmsiList.Contains(t.MMSI))
				.Select(t => t.MMSI)
				.ToListAsync();



			// Phân loại insert/update
			var toInsert = ships.Where(s => !existingMmsis.Contains(s.MMSI)).ToList();
			var toUpdate = ships.Where(s => existingMmsis.Contains(s.MMSI)).ToList();

			// Bulk Insert
			if (toInsert.Any())
			{
				ships.ForEach(s => { 
					s.CreatedAt = DateTime.Now;
					s.UpdatedAt = DateTime.Now;
				});
				await context.BulkInsertAsync(toInsert, config =>
				{
					config.SetOutputIdentity = false;
					config.BatchSize = 600; // Tối ưu theo thử nghiệm
				});
			}

			// Bulk Update
			if (toUpdate.Any())
			{
				var existingShips = await context.DM_Tau
					.Where(s => mmsiList.Contains(s.MMSI))
					.ToDictionaryAsync(s => s.MMSI);

				foreach (var ship in toUpdate)
				{
					if (existingShips.TryGetValue(ship.MMSI, out var existingShip))
					{
						ship.VesselName = string.IsNullOrEmpty(ship.VesselName) ? existingShip.VesselName : ship.VesselName;
						ship.IMONumber = (ship.IMONumber == 0 || ship.IMONumber == null) ? existingShip.IMONumber : ship.IMONumber;
						ship.CallSign = string.IsNullOrEmpty(ship.CallSign) ? existingShip.CallSign : ship.CallSign;
						ship.ShipType = (ship.ShipType == 0 || ship.ShipType == null) ? existingShip.ShipType : ship.ShipType;
						ship.AISVersion = (ship.AISVersion == 0 || ship.AISVersion == null) ? existingShip.AISVersion : ship.AISVersion;
						ship.TypeOfEPFD = (ship.TypeOfEPFD == 0 || ship.TypeOfEPFD == null) ? existingShip.TypeOfEPFD : ship.TypeOfEPFD;
						ship.ShipLength = (ship.ShipLength == 0 || ship.ShipLength == null) ? existingShip.ShipLength : ship.ShipLength;
						ship.ShipWidth = (ship.ShipWidth == 0 || ship.ShipWidth == null) ? existingShip.ShipWidth : ship.ShipWidth;
						ship.Draught = (ship.Draught == 0 || ship.Draught == null) ? existingShip.Draught : ship.Draught;
						ship.Destination = string.IsNullOrEmpty(ship.Destination) ? existingShip.Destination : ship.Destination;
						ship.VirtualAidFlag = !ship.VirtualAidFlag.HasValue ? existingShip.VirtualAidFlag : ship.VirtualAidFlag;
						ship.OffPositionIndicator = !ship.OffPositionIndicator.HasValue ? existingShip.OffPositionIndicator : ship.OffPositionIndicator;
						ship.AidType = (ship.AidType == 0 || ship.AidType == null) ? existingShip.AidType : ship.AidType;
						ship.UpdatedAt = DateTime.Now;
					}
				}
				await context.BulkUpdateAsync(toUpdate, config =>
				{
					config.BatchSize = 600;
					config.PropertiesToInclude = new List<string>
					{
						nameof(DM_Tau.VesselName),
						nameof(DM_Tau.IMONumber),
						nameof(DM_Tau.CallSign),
						nameof(DM_Tau.ShipType),
						nameof(DM_Tau.AISVersion),
						nameof(DM_Tau.TypeOfEPFD),
						nameof(DM_Tau.ShipLength),
						nameof(DM_Tau.ShipWidth),
						nameof(DM_Tau.Draught),
						nameof(DM_Tau.Destination),
						nameof(DM_Tau.VirtualAidFlag),
						nameof(DM_Tau.OffPositionIndicator),
						nameof(DM_Tau.UpdatedAt),
						nameof(DM_Tau.AidType)
					};
				});
			}
		}

		private async Task AddShipHistoryAsync(List<DM_Tau> ships, AppDBContext context)
		{
			// Lấy mã hành trình mới nhất
			var latestRoute = await context.QL_HanhTrinh
				.Where(ht => ships.Select(s => s.MMSI).Contains(ht.MMSI))
				.GroupBy(ht => ht.MMSI)
				.Select(g => new {
					MMSI = g.Key,
					MaHanhTrinh = g.Max(ht => ht.MaHanhTrinh)
				})
				.ToDictionaryAsync(x => x.MMSI, x => x.MaHanhTrinh);

			var histories = ships.Select(s => new DM_Tau_HS
			{
				MMSI = s.MMSI,
				MaHanhTrinh = latestRoute.TryGetValue(s.MMSI, out var voyage) ? voyage : 0,
				VesselName = s.VesselName,
				IMONumber = s.IMONumber,
				CallSign = s.CallSign,
				ShipType = s.ShipType,
				AISVersion = s.AISVersion,
				TypeOfEPFD = s.TypeOfEPFD,
				ShipLength = s.ShipLength,
				ShipWidth = s.ShipWidth,
				Draught = s.Draught,
				Destination = s.Destination,
				VirtualAidFlag = s.VirtualAidFlag,
				OffPositionIndicator = s.OffPositionIndicator,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				AidType = s.AidType
			}).ToList();

			await context.BulkInsertAsync(histories, config =>
			{
				config.BatchSize = 600;
			});
		}


		private async Task ProcessRoutesAsync(CancellationToken stoppingToken)
		{
			const int BATCH_SIZE = 100;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var allRoutes = _routeStore.GetAllRoute().ToList();
					//Console.WriteLine($"AllRoute: {JsonSerializer.Serialize(allRoutes)}");

					for (int i = 0; i < allRoutes.Count; i += BATCH_SIZE)
					{
						var batch = allRoutes.Skip(i).Take(BATCH_SIZE).ToList();

						using (var scope = _scopeFactory.CreateScope())
						{
							var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
							await using var transaction = await dbContext.Database.BeginTransactionAsync();

							try
							{
								await BulkProcessRoutesAsync(batch, dbContext);

								await transaction.CommitAsync();

								Console.WriteLine($"Batch: {JsonSerializer.Serialize(batch)}");
								//Console.WriteLine("Du lieu hanh trinh đa đuoc luu vao database.");
								_routeStore.DeleteMessages(batch); // Xóa batch đã xử lý

							}
							catch
							{
								await transaction.RollbackAsync();
								throw;
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in Route processing: {ex.Message}");
				}
			}
		}

		private async Task BulkProcessRoutesAsync(List<DM_HanhTrinh> routes, AppDBContext context)
		{
			var mmsiList = routes.Select(r => r.MMSI).ToList();

			// Lấy danh sách MMSI đã tồn tại trong DM_Tau
			var existingMmsis = await context.DM_Tau
				.AsNoTracking()
				.Where(t => mmsiList.Contains(t.MMSI))
				.Select(t => t.MMSI)
				.ToListAsync();

			var tausToInsert = new List<DM_Tau>();
			var hanhTrinhsToInsert = new List<DM_HanhTrinh>();

			// Xử lý các bản ghi
			foreach (var route in routes)
			{
				if (route.Longitude == null || route.Latitude == null || route.CourseOverGround == null || route.TrueHeading == null || route.Longitude == 0 || route.Latitude == 0 || route.CourseOverGround == 0 || route.TrueHeading == 0)
				{
					// Lấy giá trị từ bản ghi mới nhất nếu các giá trị là NULL
					var latestRoute = await context.QL_HanhTrinh
						.AsNoTracking()
						.Where(ht => ht.MMSI == route.MMSI)
						.Where(ht => ht.Longitude != null && ht.Latitude != null && ht.CourseOverGround != null && ht.TrueHeading != null && ht.Longitude != 0 && ht.Latitude != 0 && ht.CourseOverGround != 0 && ht.TrueHeading != 0)
						.OrderByDescending(ht => ht.DateTimeUTC)
						.FirstOrDefaultAsync();

					//Console.WriteLine($"latestRoute: {JsonSerializer.Serialize(latestRoute)}");

					if (latestRoute == null)
					{
						continue;
					}
					route.Longitude = latestRoute.Longitude;
					route.Latitude = latestRoute.Latitude;
					route.CourseOverGround = latestRoute.CourseOverGround;
					route.TrueHeading = latestRoute.TrueHeading;
				}
				 
				if (!existingMmsis.Contains(route.MMSI))
				{
					// Only add to insert if it's not already in the context
					if (!context.DM_Tau.Local.Any(t => t.MMSI == route.MMSI) && !tausToInsert.Any(t => t.MMSI == route.MMSI))
					{
						tausToInsert.Add(new DM_Tau { MMSI = route.MMSI });
					}
				}
				route.CreatedAt = DateTime.Now;
				hanhTrinhsToInsert.Add(route);

			}

			// Bulk Insert
			if (tausToInsert.Any())
			{
				await context.BulkInsertAsync(tausToInsert, config =>
				{
					config.BatchSize = 600;
				});
			}

			if (hanhTrinhsToInsert.Any())
			{
				await context.BulkInsertAsync(hanhTrinhsToInsert, config =>
				{
					config.BatchSize = 600;
				});
			}


		}



		// day la Version 1
		//private async Task ProcessShipsAsync(CancellationToken stoppingToken)
		//{
		//	while (!stoppingToken.IsCancellationRequested)
		//	{
		//		try
		//		{
		//			if (_shipStore.GetAllShip().Any())
		//			{
		//				DM_Tau? ship = _shipStore.GetAllShip().FirstOrDefault();

		//				using (var scope = _scopeFactory.CreateScope())
		//				{
		//					var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
		//					var ThamSoJSON = JsonSerializer.Serialize(ship);

		//					_logger.LogInformation($"processing ship: {ThamSoJSON}");
		//					var exec = await dbContext.ExecuteProcedureAsync(
		//						"Proc_DM_Tau_Update",
		//						ThamSoJSON,
		//						"Admin"
		//					);

		//					if (exec == null)
		//					{
		//						Console.WriteLine("Không thể chèn dữ liệu tàu.");
		//					}
		//					else
		//					{
		//						Console.WriteLine("Dữ liệu tàu đã được lưu vào database.");
		//					}
		//				}
		//				_shipStore.DeleteFirstMessage();
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			_logger.LogError($"Error in Ship processing: {ex.Message}");
		//		}

		//	}
		//}

		//private async Task ProcessRoutesAsync(CancellationToken stoppingToken)
		//{
		//	while (!stoppingToken.IsCancellationRequested)
		//	{
		//		try
		//		{
		//			if (_routeStore.GetAllRoute().Any())
		//			{
		//				DM_HanhTrinh? route = _routeStore.GetAllRoute().FirstOrDefault();

		//				using (var scope = _scopeFactory.CreateScope())
		//				{
		//					var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
		//					var ThamSoJSON = JsonSerializer.Serialize(route);

		//					//_logger.LogInformation($"processing route: {ThamSoJSON}");
		//					var exec = await dbContext.ExecuteProcedureAsync(
		//						"Proc_QL_HanhTrinh_Update",
		//						ThamSoJSON,
		//						"Admin"
		//					);

		//					if (exec == null)
		//					{
		//						Console.WriteLine("Không thể chèn dữ liệu hành trình.");
		//					}
		//					else
		//					{
		//						//Console.WriteLine("Dữ liệu hành trình đã được lưu vào database.");
		//					}
		//				}
		//				_routeStore.DeleteFirstMessage();
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			_logger.LogError($"Error in Route processing: {ex.Message}");
		//		}

		//	}
		//}


	}
}
