using Domain.Configs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ArchiveExpiredListingsService(
    IServiceScopeFactory scopeFactory,
    ILogger<ArchiveExpiredListingsService> logger
    ) : BackgroundService
{
      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredListingsArchiveService started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation($"ExpiredListingsArchiveService will be executed at: {DateTime.UtcNow.AddHours(3)}");
            // check archived listings every 3 hours
            await Task.Delay(TimeSpan.FromHours(3), stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await ArchiveExpiredListingsAsync();
            }
        }
    }

    private async Task ArchiveExpiredListingsAsync()
    {
        logger.LogInformation("Archiving expired listings...");

        using var scope = scopeFactory.CreateScope();
        var listingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository<ListingBase>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var currentDate = DateTime.UtcNow;
        var expiredListings = await listingRepository.GetListingsToArchiveAsync(currentDate);

        logger.LogInformation($"Found {expiredListings.Count} expired listings to archive");

        if (expiredListings.Count == 0)
        {
            logger.LogInformation("ExpiredListingsArchiveService finished - no listings to archive");
            return;
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var listing in expiredListings)
            {
                listing.IsArchived = true;
                listing.IsFeatured = false;
                listing.FeaturedAt = null;

                listingRepository.UpdateListing(listing);
            }
            
            // TODO: Send notification batch (Dedicated listings for user must be merged first)
            // notificationService.sendNotificationsBatch(userNotifications)

            await unitOfWork.CommitTransactionAsync();
            
            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");

            foreach (var listing in expiredListings)
            {
                var listingType = listing is ProductListing ? ResourceType.Product : ResourceType.Auction;
            
                if (listingType == ResourceType.Product)
                    await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}");
                else
                    await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{listing.Id}");
            }

            logger.LogInformation($"Successfully archived {expiredListings.Count} listings");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when archiving expired listings: {ex.Message}");
            throw;
        }
    }
}