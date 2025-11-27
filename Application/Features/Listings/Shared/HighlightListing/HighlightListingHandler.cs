using Domain.Configs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Listings.Shared.HighlightListing;

public class HighlightListingHandler(
    IListingRepository<ListingBase> listingRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<HighlightListingCommand, string>
{
    public async Task<string> Handle(HighlightListingCommand request, CancellationToken cancellationToken)
    {

        var userListings = await listingRepository.GetUserListingsAsync(request.UserId);
        var listingsToFeature = userListings.Where(l => request.ListingIds.Contains(l.Id) && l.IsFeatured == false).ToList();

        if (listingsToFeature.Count == 0)
        {
            throw new NotFoundException("No proper listings to highlight");
        }

        var currentDate = DateTime.UtcNow;
       
        
        foreach (var listingToFeature in listingsToFeature)
        {
            listingToFeature.IsFeatured = true;
            listingToFeature.FeaturedAt = currentDate;
            listingRepository.UpdateListing(listingToFeature);
        }

        await unitOfWork.SaveChangesAsync();
        await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");
        foreach (var listing in listingsToFeature)
        {
            var listingType = listing is ProductListing ? ResourceType.Product : ResourceType.Auction;
            
            if (listingType == ResourceType.Product)
                await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{listing.Id}");
            else
                await cacheService.RemoveAsync($"{CacheKeys.AUCTION_LISTING}{listing.Id}");
        }
        
        
        return $"Listings with id: {string.Join(",", listingsToFeature.Select(l => l.Id))} successfully featured"; 
    }
}