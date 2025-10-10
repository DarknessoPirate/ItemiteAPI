using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProductListings.GetProductListing;

public class GetProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICacheService cache,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ILogger<GetProductListingHandler> logger
    ) : IRequestHandler<GetProductListingQuery, ProductListingResponse>
{
    public async Task<ProductListingResponse> Handle(GetProductListingQuery request, CancellationToken cancellationToken)
    {
        var cachedListing =
            await cache.GetAsync<ProductListingResponse>($"{CacheKeys.PRODUCT_LISTINGS}{request.ListingId}");
        if (cachedListing != null)
        {
            return cachedListing;
        }
        
        var listing = await productListingRepository.GetListingWithCategoriesAndOwnerByIdAsync(request.ListingId);
        if (listing.OwnerId != currentUser.GetId())
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
        await cache.SetAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}", mappedListing);
        
        return mappedListing;
    }
}