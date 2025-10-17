using Application.Exceptions;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.UpdateProductListing;

public class UpdateProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICategoryRepository categoryRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateProductListingHandler> logger
    ) : IRequestHandler<UpdateProductListingCommand, ProductListingBasicResponse>
{
    public async Task<ProductListingBasicResponse> Handle(UpdateProductListingCommand request, CancellationToken cancellationToken)
    {
        var productListingToUpdate = await productListingRepository.GetListingByIdAsync(request.ListingId);
        if (productListingToUpdate == null)
        {
            throw new NotFoundException("Product listing with id " + request.ListingId + " not found");
        }
        
        if (productListingToUpdate.OwnerId != request.UserId)
        {
            throw new ForbiddenException("You are not allowed to update this product");
        }
        
        productListingToUpdate.Name = request.UpdateDto.Name;
        productListingToUpdate.Description = request.UpdateDto.Description;
        productListingToUpdate.Price = request.UpdateDto.Price;
        productListingToUpdate.Location = request.UpdateDto.Location;
        productListingToUpdate.IsNegotiable = request.UpdateDto.IsNegotiable ?? false;
        
        var category = await categoryRepository.GetByIdAsync(request.UpdateDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.UpdateDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to product listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        productListingToUpdate.Categories = directParents;

        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.UpdateDto.ExistingPhotoOrders != null && request.UpdateDto.ExistingPhotoIds != null)
            {
                var allListingPhotos = productListingToUpdate.ListingPhotos.ToList();
                var photosToDelete = allListingPhotos
                    .Where(lp => !request.UpdateDto.ExistingPhotoIds.Contains(lp.PhotoId))
                    .ToList();
                
                foreach (var listingPhoto in photosToDelete)
                {
                    var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.Photo.PublicId);
                    if (deletionResult.Error != null)
                    {
                        throw new CloudinaryException($"Failed to delete photo {listingPhoto.Photo.Id}: {deletionResult.Error.Message}");
                    }
                    await photoRepository.DeletePhotoAsync(listingPhoto.PhotoId);
                    productListingToUpdate.ListingPhotos.Remove(listingPhoto);
                }
               
                for (int i = 0; i < request.UpdateDto.ExistingPhotoIds.Count; i++)
                {
                    var photoId = request.UpdateDto.ExistingPhotoIds[i];
                    var newOrder = request.UpdateDto.ExistingPhotoOrders[i];
                    
                    var listingPhoto = productListingToUpdate.ListingPhotos
                        .FirstOrDefault(lp => lp.PhotoId == photoId);
        
                    if (listingPhoto != null)
                    {
                        listingPhoto.Order = newOrder;
                    }
                }
            }
            
            if (request.NewImages != null && request.NewImages.Any())
            {
                var savedPhotosPublicIds = new List<string>();
    
                for (int i = 0; i < request.NewImages.Count; i++)
                {
                    var image = request.NewImages[i];
                    var uploadResult = await mediaService.UploadPhotoAsync(image.File);
        
                    if (uploadResult.Error != null)
                    {
                        foreach (var savedPhoto in savedPhotosPublicIds)
                        {
                            await mediaService.DeleteImageAsync(savedPhoto);
                        }
                        throw new CloudinaryException(uploadResult.Error.Message);
                    }
        
                    savedPhotosPublicIds.Add(uploadResult.PublicId);
        
                    var photo = new Photo
                    {
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        PublicId = uploadResult.PublicId
                    };
        
                    await photoRepository.AddPhotoAsync(photo);
                    
                    int order = request.NewImages != null && request.NewImages.Count > i
                        ? request.NewImages.Select(i => i.Order).ToArray()[i]
                        : productListingToUpdate.ListingPhotos.Max(lp => lp.Order) + i + 1;
        
                    var listingPhoto = new ListingPhoto
                    {
                        Photo = photo,
                        Order = order
                    };
        
                    productListingToUpdate.ListingPhotos.Add(listingPhoto);
                }
            }

            productListingRepository.UpdateListing(productListingToUpdate);
            await unitOfWork.CommitTransactionAsync();

            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTINGS}*");
            await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");

            return mapper.Map<ProductListingBasicResponse>(productListingToUpdate);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when updating {request.ListingId}: {ex.Message}");
            throw;
        }
    }
}