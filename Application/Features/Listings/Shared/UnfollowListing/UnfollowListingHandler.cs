using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Listings.Shared.UnfollowListing;

public class UnfollowListingHandler(
    IListingRepository<ListingBase> listingRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    INotificationService notificationService
    ) : IRequestHandler<UnfollowListingCommand>
{
    public async Task Handle(UnfollowListingCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
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
            
        var notificationInfo = new NotificationInfo
        {
            Message = $"User {user.UserName} has unfollowed your listing {listingToUnfollow.Name}.",
            UserId = user.Id,
            ResourceType = ResourceType.User.ToString(),
            NotificationImageUrl = user.ProfilePhoto?.Url,
            UserInfo = new ChatMemberInfo
            {
                Id = user.Id,
                UserName = user.UserName!,
                PhotoUrl = user.ProfilePhoto?.Url
            }
        };
            
        await notificationService.SendNotification([listingToUnfollow.OwnerId], request.UserId, notificationInfo);

        await unitOfWork.SaveChangesAsync();

        await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}{request.UserId}_followed*");
    }
}