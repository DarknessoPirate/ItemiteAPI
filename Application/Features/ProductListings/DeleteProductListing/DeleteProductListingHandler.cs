using Application.Exceptions;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.DeleteProductListing;

public class DeleteProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IMediaService mediaService,
    ILogger<DeleteProductListingHandler> logger
    ) : IRequestHandler<DeleteProductListingCommand>
{
    public async Task Handle(DeleteProductListingCommand request, CancellationToken cancellationToken)
    {
        var listingToDelete = await productListingRepository.GetListingWithPhotosByIdAsync(request.ListingId);
        if (listingToDelete.OwnerId != request.UserId)
        {
            throw new ForbiddenException("You are not allowed to delete this listing");
        }
        
        var photosToDelete = listingToDelete.ListingPhotos.Select(p => p.Photo).ToList();
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
                productListingRepository.DeleteListing(listingToDelete);
            }

            await unitOfWork.CommitTransactionAsync();
            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTINGS}*");
            await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when deleting product listing {listingToDelete.Id}: {ex.Message}");
            throw;
        }
    }
}