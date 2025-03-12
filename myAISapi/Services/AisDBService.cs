using Microsoft.AspNetCore.Routing;
using myAISapi.Data;
using myAISapi.Decoder;
using myAISapi.Models;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace myAISapi.Services
{
	public class AisDBService : BackgroundService
	{
		private readonly ILogger<AisDBService> _logger;
		private readonly IDecodedAISStore _decodedAISStore;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IDM_Tau_Store _shipStore;
		private readonly IDM_Tau_HS_Store _shipHsStore;
		private readonly IDM_HanhTrinh_Store _routeStore;

		public AisDBService(
			//AisDecoderService aisDecoderService,
			ILogger<AisDBService> logger,
			IDecodedAISStore decodedAISStore,
			IServiceScopeFactory scopeFactory,
			IDM_Tau_Store shipStore,
			IDM_Tau_HS_Store shipHsStore,
			IDM_HanhTrinh_Store routeStore)
		{
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

			var shipTask = ProcessShipsAsync(stoppingToken);
			var routeTask = ProcessRoutesAsync(stoppingToken);

			await Task.WhenAll(shipTask, routeTask);

			_logger.LogInformation("AIS DB Hosted Service is stopping.");
		}


		private async Task ProcessShipsAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					if (_shipStore.GetAllShip().Any())
					{
						DM_Tau? ship = _shipStore.GetAllShip().FirstOrDefault();

						using (var scope = _scopeFactory.CreateScope())
						{
							var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
							var ThamSoJSON = JsonSerializer.Serialize(ship);

							_logger.LogInformation($"processing ship: {ship}");
							var exec = await dbContext.ExecuteProcedureAsync(
								"Proc_DM_Tau_Update",
								ThamSoJSON,
								"Admin"
							);

							if (exec == null)
							{
								Console.WriteLine("Không thể chèn dữ liệu tàu.");
							}
							else
							{
								Console.WriteLine("Dữ liệu tàu đã được lưu vào database.");
							}
						}
						_shipStore.DeleteFirstMessage();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in Ship processing: {ex.Message}");
				}

				await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
			}
		}

		private async Task ProcessRoutesAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					if (_routeStore.GetAllRoute().Any())
					{
						DM_HanhTrinh? route = _routeStore.GetAllRoute().FirstOrDefault();

						using (var scope = _scopeFactory.CreateScope())
						{
							var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
							var ThamSoJSON = JsonSerializer.Serialize(route);

							_logger.LogInformation($"processing route: {ThamSoJSON}");
							var exec = await dbContext.ExecuteProcedureAsync(
								"Proc_QL_HanhTrinh_Update",
								ThamSoJSON,
								"Admin"
							);

							if (exec == null)
							{
								Console.WriteLine("Không thể chèn dữ liệu hành trình.");
							}
							else
							{
								Console.WriteLine("Dữ liệu hành trình đã được lưu vào database.");
							}
						}
						_routeStore.DeleteFirstMessage();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in Route processing: {ex.Message}");
				}

				await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
			}
		}


		//private string ThamSo(int MessageType, )
		//{
		//	return JsonSerializer.Serialize(new
		//	{
		//		raw_message = message,
		//		received_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
		//	});
		//}
	}
}
