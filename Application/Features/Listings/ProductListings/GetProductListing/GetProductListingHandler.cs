using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.ProductListings.GetProductListing;

public class GetProductListingHandler(
    IListingRepository<ProductListing> productListingRepository,
    IListingRepository<ListingBase> listingRepository,
    ILIstingViewRepository listingViewRepository,
    ICacheService cache,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<GetProductListingHandler> logger
    ) : IRequestHandler<GetProductListingQuery, ProductListingResponse>
{
    public async Task<ProductListingResponse> Handle(GetProductListingQuery request,
        CancellationToken cancellationToken)
    {
        var cachedListing =
            await cache.GetAsync<ProductListingResponse>($"{CacheKeys.PRODUCT_LISTING}{request.ListingId}");
        if (cachedListing != null)
        {
            if (request.UserId != null)
            {
                var followedListings = await listingRepository.GetUserFollowedListingsAsync(request.UserId.Value);
                cachedListing.IsFollowed = followedListings.Select(f => f.ListingId).Contains(request.ListingId);
            }
            return cachedListing;
        }

        var listing = await productListingRepository.GetListingByIdAsync(request.ListingId);
        if (listing == null)
        {
            throw new NotFoundException($"Product listing with id: {request.ListingId} not found");
        }

        listing.ViewsCount += 1;
        productListingRepository.UpdateListing(listing);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // TODO: ALLOW ANONYMOUS USER FETCHING
        if (request.UserId != null && listing.OwnerId != request.UserId)
        {
            try
            {
                var listingView = new ListingView
                {
                    ListingId = listing.Id,
                    UserId = request.UserId.Value,
                    RootCategoryId = listing.Categories.FirstOrDefault(c => c.RootCategoryId == null)?.Id
                                     ?? throw new InvalidOperationException("No root category found"),
                    ViewedAt = DateTime.UtcNow
                };
                await listingViewRepository.AddAsync(listingView);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error when updating listing {listing.Id} views: {ex.Message}");
            }
        }

        var mappedListing = mapper.Map<ProductListingResponse>(listing);
        
        var listingImages = listing.ListingPhotos;
        var listingImageResponses = listingImages.Select(x => new ListingImageResponse
        {
            ImageOrder = x.Order,
            ImageUrl = x.Photo.Url,
            ImageId = x.PhotoId
        }).ToList();

        mappedListing.Images = listingImageResponses;
        
        // set cache before setting isFollowed value
        await cache.SetAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}", mappedListing, 5);
        
        if (request.UserId != null)
        {
            var followedListings = await listingRepository.GetUserFollowedListingsAsync(request.UserId.Value);
            mappedListing.IsFollowed = followedListings.Select(f => f.ListingId).Contains(request.ListingId);
        }
        
        return mappedListing;
    }
}