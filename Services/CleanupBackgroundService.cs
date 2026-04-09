using AurHER.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AurHER.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CleanupBackgroundService> _logger;

        public CleanupBackgroundService(IServiceProvider services, ILogger<CleanupBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    
                    using var scope = _services.CreateScope();
                    var cleanupService = scope.ServiceProvider.GetRequiredService<ICleanupService>();
                    
                    _logger.LogInformation("Running scheduled cleanup...");
                    var count = await cleanupService.CleanupStaleOrdersAsync(20);
                    _logger.LogInformation($"Scheduled cleanup completed. {count} orders cleaned.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduled cleanup");
                }
            }
            
            _logger.LogInformation("Cleanup Background Service stopped");
        }
    }
}