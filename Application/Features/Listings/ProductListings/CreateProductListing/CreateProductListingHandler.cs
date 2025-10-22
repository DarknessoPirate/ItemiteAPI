using AutoMapper;
using Domain.Configs;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.ProductListings.CreateProductListing;

public class CreateProductListingHandler(
        IListingRepository<ProductListing> productListingRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IMediaService mediaService,
        IPhotoRepository photoRepository,
        UserManager<User> userManager,
        ILogger<CreateProductListingHandler> logger
        ) : IRequestHandler<CreateProductListingCommand, int>
{
   
    public async Task<int> Handle(CreateProductListingCommand request, CancellationToken cancellationToken)
    {
        var productListing = mapper.Map<ProductListing>(request.ProductListingDto);
        productListing.OwnerId = request.UserId;
        
        if (request.ProductListingDto.Location == null || !IsLocationComplete(request.ProductListingDto.Location))
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user?.Location == null || !IsLocationComplete(user.Location))
            {
                throw new BadRequestException("Location is required. Please provide location or set your profile location.");
            }
            
            productListing.Location = new Location
            {
                Longitude = user.Location.Longitude,
                Latitude = user.Location.Latitude,
                Country = user.Location.Country,
                City = user.Location.City,
                State = user.Location.State
            };
        }
        
        var category = await categoryRepository.GetByIdAsync(request.ProductListingDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.ProductListingDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to product listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        productListing.Categories = directParents;
        
        var savedPhotosPublicIds = new List<string>();
        var listingPhotos = new List<ListingPhoto>();
        await unitOfWork.BeginTransactionAsync();
        try
        {
            int orderIndex = 0;
            foreach (var image in request.Images)
            {
                var uploadResult = await mediaService.UploadPhotoAsync(image);
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
                
                var listingPhoto = new ListingPhoto
                {
                    Photo = photo,
                    Order = request.ProductListingDto.ImageOrders[orderIndex],
                };
                listingPhotos.Add(listingPhoto);
                orderIndex++;
            }

            productListing.ListingPhotos = listingPhotos;
            await productListingRepository.CreateListingAsync(productListing);
            await unitOfWork.CommitTransactionAsync();

            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
        }
        catch (Exception ex)
        {
            foreach (var savedPhotos in savedPhotosPublicIds)
            {
                await mediaService.DeleteImageAsync(savedPhotos);
            }

            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when creating new product listing: {ex.Message}");
            throw;
        }
        
        
        return productListing.Id;
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