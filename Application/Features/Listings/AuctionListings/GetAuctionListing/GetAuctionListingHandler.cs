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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Listings.AuctionListings.GetAuctionListing;

public class GetAuctionListingHandler(
    IListingRepository<AuctionListing> auctionListingRepository,
    IListingRepository<ListingBase> listingRepository,
    ILIstingViewRepository listingViewRepository,
    ICacheService cache,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<GetProductListingHandler> logger
    ) : IRequestHandler<GetAuctionListingQuery, AuctionListingResponse>
{
    public async Task<AuctionListingResponse> Handle(GetAuctionListingQuery request,
        CancellationToken cancellationToken)
    {
        var cachedListing =
            await cache.GetAsync<AuctionListingResponse>($"{CacheKeys.AUCTION_LISTING}{request.ListingId}");
        if (cachedListing != null)
        {
            if (request.UserId != null)
            {
                var followedListings = await listingRepository.GetUserFollowedListingsAsync(request.UserId.Value);
                cachedListing.IsFollowed = followedListings.Select(f => f.ListingId).Contains(request.ListingId);
            }
            return cachedListing;
        }

        var listing = await auctionListingRepository.GetListingByIdAsync(request.ListingId);
        if (listing == null)
        {
            throw new NotFoundException($"Auction with id: {request.ListingId} not found");
        }

        listing.ViewsCount += 1;
        auctionListingRepository.UpdateListing(listing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: allow for anonymous user fetching
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
                logger.LogError(ex, $"Error when adding listingview for listing {listing.Id}: {ex.Message}");
            }
        }

        var mappedListing = mapper.Map<AuctionListingResponse>(listing);
        
        var listingImages = listing.ListingPhotos;
        var listingImageResponses = listingImages.Select(x => new ListingImageResponse
        {
            ImageOrder = x.Order,
            ImageUrl = x.Photo.Url,
            ImageId = x.PhotoId
        }).ToList();

        mappedListing.Images = listingImageResponses;
        
        // set cache before setting isFollowed value
        await cache.SetAsync($"{CacheKeys.AUCTION_LISTING}{listing.Id}", mappedListing, 5);
        
        if (request.UserId != null)
        {
            var followedListings = await listingRepository.GetUserFollowedListingsAsync(request.UserId.Value);
            mappedListing.IsFollowed = followedListings.Select(f => f.ListingId).Contains(request.ListingId);
        }
        
        return mappedListing;
    }
}