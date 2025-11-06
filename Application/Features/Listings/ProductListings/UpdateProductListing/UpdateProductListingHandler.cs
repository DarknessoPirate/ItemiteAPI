using Application.Exceptions;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.ProductListings.UpdateProductListing;

public class UpdateProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICategoryRepository categoryRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    ICacheService cacheService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateProductListingHandler> logger
    ) : IRequestHandler<UpdateProductListingCommand, ProductListingResponse>
{
    public async Task<ProductListingResponse> Handle(UpdateProductListingCommand request, CancellationToken cancellationToken)
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
        if (request.UpdateDto.Location == null || !IsLocationComplete(request.UpdateDto.Location))
        {
            var ownerLoc = productListingToUpdate.Owner.Location;
            if (ownerLoc == null || !IsLocationComplete(ownerLoc))
            {
                throw new BadRequestException("Location is required. Please provide location or set your profile location.");
            }
   
            productListingToUpdate.Location.Longitude = ownerLoc.Longitude;
            productListingToUpdate.Location.Latitude = ownerLoc.Latitude;
            productListingToUpdate.Location.Country = ownerLoc.Country;
            productListingToUpdate.Location.City = ownerLoc.City;
            productListingToUpdate.Location.State = ownerLoc.State;
        }
        else
        {
            var loc = request.UpdateDto.Location;
            productListingToUpdate.Location.Longitude = loc.Longitude;
            productListingToUpdate.Location.Latitude = loc.Latitude;
            productListingToUpdate.Location.Country = loc.Country;
            productListingToUpdate.Location.City = loc.City;
            productListingToUpdate.Location.State = loc.State;
        }
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
            var allListingPhotos = productListingToUpdate.ListingPhotos.ToList();
            if (request.UpdateDto.ExistingPhotoOrders == null && request.UpdateDto.ExistingPhotoIds == null)
            {
                foreach (var listingPhoto in allListingPhotos)
                {
                    var deletionResult = await mediaService.DeleteImageAsync(listingPhoto.Photo.PublicId);
                    if (deletionResult.Error != null)
                    {
                        throw new CloudinaryException($"Failed to delete photo {listingPhoto.Photo.Id}: {deletionResult.Error.Message}");
                    }
                    await photoRepository.DeletePhotoAsync(listingPhoto.PhotoId);
                    productListingToUpdate.ListingPhotos.Remove(listingPhoto);
                }
            }
            else if (request.UpdateDto.ExistingPhotoOrders != null && request.UpdateDto.ExistingPhotoIds != null)
            {
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
                        FileName = image.File.FileName,
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

            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTING}*_{request.ListingId}");

            return mapper.Map<ProductListingResponse>(productListingToUpdate);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when updating {request.ListingId}: {ex.Message}");
            throw;
        }
    }
    
    private bool IsLocationComplete(Location? location)
    {
        if (location == null) return false;
    
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.State);
    }
}