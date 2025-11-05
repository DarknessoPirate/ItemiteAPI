using Application.Features.Listings.ProductListings.GetProductListing;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.AuctionListing;
using Domain.DTOs.Listing;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.AuctionListings.GetAuctionListing;

public class GetAuctionListingHandler(
    IListingRepository<AuctionListing> auctionListingRepository,
    IListingRepository<ListingBase> listingRepository,
    ICacheService cache,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<GetProductListingHandler> logger
    ) : IRequestHandler<GetAuctionListingQuery, AuctionListingResponse>
{
    public async Task<AuctionListingResponse> Handle(GetAuctionListingQuery request, CancellationToken cancellationToken)
    {
        var cachedListing =
            await cache.GetAsync<AuctionListingResponse>($"{CacheKeys.AUCTION_LISTING}{request.UserId.ToString() ?? "null"}_{request.ListingId}");
        if (cachedListing != null)
        {
            return cachedListing;
        }
        
        var listing = await auctionListingRepository.GetListingByIdAsync(request.ListingId);
        if (listing == null)
        {
            throw new NotFoundException($"Auction with id: {request.ListingId} not found");
        }
        if (request.UserId != null && listing.OwnerId != request.UserId)
        {
            try
            {
                listing.Views += 1;
                auctionListingRepository.UpdateListing(listing);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error when updating listing {listing.Id} views: {ex.Message}");
            }
        }
        
        var mappedListing = mapper.Map<AuctionListingResponse>(listing);
        
        if (request.UserId != null)
        {
            var followedListings = await listingRepository.GetUserFollowedListingsAsync(request.UserId.Value);
            mappedListing.IsFollowed = followedListings.Select(f => f.ListingId).Contains(request.ListingId);
        }
        
        var listingImages = listing.ListingPhotos;
        var listingImageResponses = listingImages.Select(x => new ListingImageResponse
        {
            ImageOrder = x.Order,
            ImageUrl = x.Photo.Url,
            ImageId = x.PhotoId
        }).ToList();
        
        mappedListing.Images = listingImageResponses;
        
        await cache.SetAsync($"{CacheKeys.AUCTION_LISTING}{request.UserId.ToString() ?? "null"}_{listing.Id}", mappedListing);
        
        return mappedListing;
    }
}