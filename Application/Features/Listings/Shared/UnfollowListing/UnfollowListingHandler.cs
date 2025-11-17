using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Listings.Shared.UnfollowListing;

public class UnfollowListingHandler(
    IListingRepository<ListingBase> listingRepository,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    INotificationService notificationService
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
        
        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:4200";
        var notificationUrl = listingToUnfollow is ProductListing
            ? $"{frontendBaseUrl}/product-listings/{request.ListingId}"
            : $"{frontendBaseUrl}/auction-listings/{request.ListingId}";
            
        var notificationInfo = new NotificationInfo
        {
            Message = $"User {user.UserName} has unfollowed your listing {listingToUnfollow.Name}.",
            UrlToResource = notificationUrl,
            NotificationImageUrl = listingToUnfollow.ListingPhotos.First(p => p.Order == 1).Photo.Url
        };
            
        await notificationService.SendNotification([listingToUnfollow.OwnerId], request.UserId, notificationInfo);

        await unitOfWork.SaveChangesAsync();
    }
}