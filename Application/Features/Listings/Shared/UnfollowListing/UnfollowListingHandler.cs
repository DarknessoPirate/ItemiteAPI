using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Listings.Shared.UnfollowListing;

public class UnfollowListingHandler(
    IListingRepository<ListingBase> listingRepository,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UnfollowListingCommand>
{
    public async Task Handle(UnfollowListingCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var listingToUnfollow = await listingRepository.GetListingByIdAsync(request.ListingId);
        if (listingToUnfollow == null)
        {
            throw new NotFoundException("Listing not found");
        }
        
        var followedListings = await listingRepository.GetUserFollowedListingsAsync(user.Id);

        var followedListingToUnfollow = followedListings.Find(fl => fl.ListingId == request.ListingId);

        if (followedListingToUnfollow == null)
        {
            throw new BadRequestException("You are not following this listing");
        }

        listingToUnfollow.Followers -= 1;
        listingRepository.UnfollowListing(followedListingToUnfollow);
        listingRepository.UpdateListing(listingToUnfollow);

        await unitOfWork.SaveChangesAsync();
    }
}