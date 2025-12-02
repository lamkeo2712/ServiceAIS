using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace myAISapi.Services
{
	public class BeaconDriftHostedService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<BeaconDriftHostedService> _logger;

		public BeaconDriftHostedService(IServiceProvider serviceProvider, ILogger<BeaconDriftHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var svc = scope.ServiceProvider.GetRequiredService<BeaconDriftService>();

					await svc.ScanAllBeaconsAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error while scanning beacon drift");
				}

				await Task.Delay(TimeSpan.FromMinutes(0.5), stoppingToken);
			}
		}
	}
}
