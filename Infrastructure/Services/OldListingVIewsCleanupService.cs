using Infrastructure.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;
public class OldListingViewsCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OldListingViewsCleanupService> logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OldListingViewsCleanupService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next3Am = GetNext3Am(now);
            
            logger.LogInformation($"OldListingViewsCleanupService will be executed at: {next3Am}");
            
            var delay = next3Am - now;
            
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await CleanupOldListingViewsAsync();
            }
        }
    }
    
    private async Task CleanupOldListingViewsAsync()
    {
        logger.LogInformation("Cleaning up old listing views...");
        using var scope = scopeFactory.CreateScope();
        var listingViewRepository = scope.ServiceProvider.GetRequiredService<ILIstingViewRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        // views older than 14 days will be removed from database
        var expirationDate = DateTime.UtcNow.AddDays(-14);
        
        logger.LogInformation($"Clicks before date: {expirationDate.ToString("g")} will be removed");
        
        var expiredViews = await listingViewRepository.GetExpiredListingViewsAsync(expirationDate);
        logger.LogInformation($"Expired listing views count: {expiredViews.Count}");
        if (expiredViews.Count == 0)
        {
            logger.LogInformation("OldListingViewsCleanupService finished without removing any listing views.");
            return;
        }
        
        listingViewRepository.RemoveRange(expiredViews);
        
        await unitOfWork.SaveChangesAsync();
        
        logger.LogInformation("ExpiredFeaturedListingsCleanupService finished");
    }
    
    private static DateTime GetNext3Am(DateTime now)
    {
        var next3Am = now.Date.AddHours(3);
        if (now >= next3Am)
        {
            next3Am = next3Am.AddDays(1);
        }
        return next3Am;
    }
}