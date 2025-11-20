using Application.Exceptions;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.Shared.DeleteListing;

public class DeleteListingHandler(
    IListingRepository<ListingBase> listingRepository,
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
        if (listingToDelete.OwnerId != request.UserId)
        {
            throw new ForbiddenException("You are not allowed to delete this listing");
        }
        
        var photosToDelete = listingToDelete.ListingPhotos.Select(p => p.Photo).ToList();
        
        // get listing name and followers before deleting a listing
        var listingName = listingToDelete.Name;
        var followers = await listingRepository.GetListingFollowersAsync(request.ListingId);
        
        await unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var listingPhoto in photosToDelete)
            {
                var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.PublicId);
                if (deletionResult.Error != null)
                {
                    throw new CloudinaryException($"An error occured while deleting the photo: {deletionResult.Error.Message}");
                }
                await photoRepository.DeletePhotoAsync(listingPhoto.Id);
                listingRepository.DeleteListing(listingToDelete);
            }

            await unitOfWork.CommitTransactionAsync();
            
            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
            await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
            await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{request.ListingId}");
            
            var notificationInfo = new NotificationInfo
            {
                Message = $"Listing {listingName} has been deleted.",
            };
            
            await notificationService.SendNotification(followers.Select(f => f.Id).ToList(), request.UserId, notificationInfo);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when deleting listing {listingToDelete.Id}: {ex.Message}");
            throw;
        }
    }
}