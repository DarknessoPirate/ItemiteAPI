using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.ProductListings.GetProductListing;

public class GetProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICacheService cache,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<GetProductListingHandler> logger
    ) : IRequestHandler<GetProductListingQuery, ProductListingResponse>
{
    public async Task<ProductListingResponse> Handle(GetProductListingQuery request, CancellationToken cancellationToken)
    {
        var cachedListing =
            await cache.GetAsync<ProductListingResponse>($"{CacheKeys.LISTING}{request.ListingId}");
        if (cachedListing != null)
        {
            return cachedListing;
        }
        
        var listing = await productListingRepository.GetListingByIdAsync(request.ListingId);
        if (request.UserId != null && listing.OwnerId != request.UserId)
        {
            try
            {
                listing.Views += 1;
                productListingRepository.UpdateListing(listing);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error when updating listing {listing.Id} views: {ex.Message}");
            }
        }
        
        var mappedListing = mapper.Map<ProductListingResponse>(listing);
        
        var listingImages = listing.ListingPhotos;
        var listingImageResponses = listingImages.Select(x => new ProductListingImageResponse
        {
            ImageOrder = x.Order,
            ImageUrl = x.Photo.Url,
            ImageId = x.PhotoId
        }).ToList();
        
        mappedListing.Images = listingImageResponses;
        
        await cache.SetAsync($"{CacheKeys.LISTING}{listing.Id}", mappedListing);
        
        return mappedListing;
    }
}