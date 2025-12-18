using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Listings.Shared.FollowListing;

public class FollowListingHandler(
    IListingRepository<ListingBase> listingRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    INotificationService notificationService
    ) : IRequestHandler<FollowListingCommand, int>
{
    public async Task<int> Handle(FollowListingCommand request, CancellationToken cancellationToken)
    {
        
        var user = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
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
            
        var notificationInfo = new NotificationInfo
        {
            Message = $"User {user.UserName} has followed your listing {listingToFollow.Name}.",
            UserId = user.Id,
            ResourceType = ResourceType.User,
            NotificationImageUrl = user.ProfilePhoto?.Url
        };
            
        await notificationService.SendNotification([listingToFollow.OwnerId], request.UserId, notificationInfo);
        
        await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}{request.UserId}_followed*");
        
        return followedListing.Id;
    }
}