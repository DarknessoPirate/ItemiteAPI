using AutoMapper;
using Domain.Configs;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.ProductListings.GetProductListing;

public class GetProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    ICacheService cache,
    IMapper mapper
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
        var mappedListing = mapper.Map<ProductListingResponse>(listing);
        await cache.SetAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}", mappedListing);
        
        return mappedListing;
    }
}