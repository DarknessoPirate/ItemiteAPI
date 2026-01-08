using Application.Exceptions;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.Shared.ArchiveListing;

public class ArchiveListingHandler(
    IListingRepository<ListingBase> listingRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    INotificationService notificationService,
    ILogger<ArchiveListingHandler> logger
    ) : IRequestHandler<ArchiveListingCommand>
{
    public async Task Handle(ArchiveListingCommand request, CancellationToken cancellationToken)
    {
        var listingToArchive = await listingRepository.GetListingByIdAsync(request.ListingId);
        if (listingToArchive == null)
        {
            throw new NotFoundException($"Listing with id: {request.ListingId} not found");
        }

        if (request.UserId != listingToArchive.OwnerId)
        {
            throw new ForbiddenException("You are not the owner of this listing");
        }
        
      
        var followers = await listingRepository.GetListingFollowersAsync(request.ListingId);
        var listingType = listingToArchive is ProductListing ? ResourceType.Product : ResourceType.Auction;
        
        await unitOfWork.BeginTransactionAsync();
        try
        {
            listingToArchive.IsArchived = true;
            // archived listing can't be featured
            listingToArchive.IsFeatured = false;
            listingToArchive.FeaturedAt = null;

            listingRepository.UpdateListing(listingToArchive);

            var notificationInfo = new NotificationInfo
            {
                Message = $"Listing {listingToArchive.Name} has been archived.",
                ListingId = listingToArchive.Id,
                ResourceType = listingType.ToString(),
                NotificationImageUrl = listingToArchive.ListingPhotos.FirstOrDefault(lp => lp.Order == 1)?.Photo.Url
            };
            
            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
            if (listingType == ResourceType.Product)
                await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
            else
                await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{request.ListingId}");

            await unitOfWork.CommitTransactionAsync();
            
            await notificationService.SendNotification(followers.Select(f => f.Id).ToList(), request.UserId,
                notificationInfo);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when archiving listing {listingToArchive.Id}: {ex.Message}");
            throw;
        }
       
    }
}