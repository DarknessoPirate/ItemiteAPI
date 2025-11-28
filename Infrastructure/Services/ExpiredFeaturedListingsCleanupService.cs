using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ExpiredFeaturedListingsCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredFeaturedListingsCleanupService> logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredFeaturedListingsCleanupService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next3Am = GetNext3Am(now);
            
            logger.LogInformation($"ExpiredFeaturedListingsCleanupService will be executed at: {next3Am}");
            
            var delay = next3Am - now;
            
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await CleanupExpiredFeaturedListingsAsync();
            }
        }
    }
    
    private async Task CleanupExpiredFeaturedListingsAsync()
    {
        logger.LogInformation("Cleaning up expired featured listings...");
        using var scope = scopeFactory.CreateScope();
        var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository<ListingBase>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        
        var expirationDate = DateTime.UtcNow.AddDays(-7);

        var expiredFeaturedListings = await listingRepository.GetExpiredFeaturedListingsAsync(expirationDate);
        logger.LogInformation($"Expired featured listings count: {expiredFeaturedListings.Count}");
        
        if (expiredFeaturedListings.Count == 0)
        {
            logger.LogInformation("ExpiredFeaturedListingsCleanupService finished without updating any listings");
            return;
        }
        
        logger.LogInformation($"Listings featured before: {expirationDate.ToString("g")} will be updated");
       
        foreach (var listing in expiredFeaturedListings)
        {
            listing.IsFeatured = false;
            listing.FeaturedAt = null;
            listingRepository.UpdateListing(listing);
        }
        await unitOfWork.SaveChangesAsync();

        var userNotifications = new Dictionary<int, NotificationInfo>();
        foreach (var listing in expiredFeaturedListings)
        {
            userNotifications[listing.OwnerId] = new NotificationInfo
            {
                Message = $"Your listing {listing.Name} is no longer featured",
                NotificationImageUrl = listing.ListingPhotos.First(lp => lp.Order == 1).Photo.Url,
                ResourceId = listing.Id,
                ResourceType = listing is ProductListing ? ResourceType.Product : ResourceType.Auction
            };
        }
        await notificationService.SendNotificationsBatch(userNotifications);
        
        await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
        foreach (var listing in expiredFeaturedListings)
        {
            var listingType = listing is ProductListing ? ResourceType.Product : ResourceType.Auction;
            
            if (listingType == ResourceType.Product)
                await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}");
            else
                await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{listing.Id}");
        }
        
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