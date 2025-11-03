using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Listings.Shared.FollowListing;

public class FollowListingHandler(
    IListingRepository<ListingBase> listingRepository,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    ICacheService cacheService
    ) : IRequestHandler<FollowListingCommand, int>
{
    public async Task<int> Handle(FollowListingCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var listingToFollow = await listingRepository.GetListingByIdAsync(request.ListingId);
        if (listingToFollow == null)
        {
            throw new NotFoundException("Listing not found");
        }

        var followers = await listingRepository.GetListingFollowersAsync(request.ListingId);
        if (followers.Contains(user))
        {
            throw new BadRequestException("Listing already followed");
        }

        if (listingToFollow.OwnerId == user.Id)
        {
            throw new BadRequestException("You cannot follow your own listing");
        }

        var followedListing = new FollowedListing
        {
            UserId = user.Id,
            ListingId = request.ListingId,
            RootCategoryId = listingToFollow.Categories.First(c => c.RootCategoryId == null).Id
        };
        
        listingToFollow.Followers += 1;
        
        await listingRepository.AddListingToFollowedAsync(followedListing);
        listingRepository.UpdateListing(listingToFollow);
        
        await unitOfWork.SaveChangesAsync();
        
        await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}{request.UserId}*");
        await cacheService.RemoveByPatternAsync($"{CacheKeys.PRODUCT_LISTING}{request.UserId}_{request.ListingId}");
        await cacheService.RemoveByPatternAsync($"{CacheKeys.AUCTION_LISTING}{request.UserId}_{request.ListingId}");
        
        return followedListing.Id;
    }
}