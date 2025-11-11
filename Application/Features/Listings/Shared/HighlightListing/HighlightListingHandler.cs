using Domain.Configs;
using Domain.Entities;
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
        foreach (var listingId in listingsToFeature.Select(l => l.Id))
        {
            await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTING}{listingId}");
            await cacheService.RemoveByPatternAsync($"{CacheKeys.AUCTION_LISTING}{listingId}");
        }
        
        
        return $"Listings with id: {string.Join(",", listingsToFeature.Select(l => l.Id))} successfully featured"; 
    }
}