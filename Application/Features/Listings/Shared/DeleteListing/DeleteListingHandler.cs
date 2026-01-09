using Application.Exceptions;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.Shared.DeleteListing;

public class DeleteListingHandler(
    IListingRepository<ListingBase> listingRepository,
    IPaymentRepository paymentRepository,
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IMediaService mediaService,
    INotificationService notificationService,
    ILogger<DeleteListingHandler> logger
    ) : IRequestHandler<DeleteListingCommand>
{
    public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listingToDelete = await listingRepository.GetListingWithPhotosByIdAsync(request.ListingId);
        if (listingToDelete == null)
        {
            throw new NotFoundException($"Listing with id: {request.ListingId} not found");
        }

        
        
        var photosToDelete = listingToDelete.ListingPhotos.Select(p => p.Photo).ToList();
        
        // get listing name and followers before deleting a listing
        var listingName = listingToDelete.Name;
        var followers = await listingRepository.GetListingFollowersAsync(request.ListingId);
        var ownerId = listingToDelete.OwnerId;
        var listingType = listingToDelete is ProductListing ? ResourceType.Product : ResourceType.Auction;

        bool hasBeenArchived;
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var payment = await paymentRepository.FindByListingIdAsync(request.ListingId);
            if (payment != null)
            {
                listingToDelete.IsArchived = true;
                listingToDelete.IsFeatured = false;
                listingToDelete.FeaturedAt = null;
                
                listingRepository.UpdateListing(listingToDelete);
                
                hasBeenArchived = true;
            }
            else
            {
                foreach (var listingPhoto in photosToDelete)
                {
                    var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.PublicId);
                    if (deletionResult.Error != null)
                    {
                        throw new CloudinaryException($"An error occured while deleting the photo: {deletionResult.Error.Message}");
                    }
                    await photoRepository.DeletePhotoAsync(listingPhoto.Id);
                }
            
                listingRepository.DeleteListing(listingToDelete);
                hasBeenArchived = false;
            }
            
            await unitOfWork.CommitTransactionAsync();
            
            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
            if (listingType == ResourceType.Product)
                await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
            else
                await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{request.ListingId}");

            var notifiactionRecipients = followers.Select(f => f.Id).ToList();
            notifiactionRecipients.Add(ownerId);
            
            var notificationInfo = new NotificationInfo
            {
                Message = hasBeenArchived ? $"Listing {listingName} has been deleted" : $"Listing {listingName} has been archived",
                ResourceType = listingToDelete is ProductListing ? ResourceType.Product.ToString() : ResourceType.Auction.ToString(),
            };
            
            await notificationService.SendNotification(notifiactionRecipients, request.UserId, notificationInfo);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when deleting listing {listingToDelete.Id}: {ex.Message}");
            throw;
        }
    }
}