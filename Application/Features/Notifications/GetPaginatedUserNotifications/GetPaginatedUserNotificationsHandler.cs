using AutoMapper;
using Domain.DTOs.Notifications;
using Domain.DTOs.Pagination;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Notifications.GetPaginatedUserNotifications;

public class GetPaginatedUserNotificationsHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IListingRepository<ListingBase> listingRepository,
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetPaginatedUserNotificationsQuery, PageResponse<NotificationInfo>>
{
    public async Task<PageResponse<NotificationInfo>> Handle(GetPaginatedUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var queryable =  notificationRepository.GetUserNotificationsQueryable(request.UserId);
        
        int totalItems = await queryable.CountAsync(cancellationToken);
        
        queryable = queryable
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize);

        var notifications = await queryable.ToListAsync(cancellationToken);
        
        var mappedNotifications = mapper.Map<List<NotificationInfo>>(notifications, opt =>
        {
            opt.Items["UserId"] = request.UserId;
        });
        
        var listingIds = mappedNotifications
            .Where(n => (n.ResourceType == "Auction" || n.ResourceType == "Product") && n.ListingId.HasValue)
            .Select(n => n.ListingId!.Value)
            .Distinct()
            .ToList();

        var userIds = mappedNotifications
            .Where(n => (n.ResourceType == "ChatPage" || n.ResourceType == "User") && n.UserId.HasValue)
            .Select(n => n.UserId!.Value)
            .Distinct()
            .ToList();
        
        var listingUrls = listingIds.Any() 
            ? await listingRepository.GetListingImageUrlsAsync(listingIds) 
            : new Dictionary<int, string>();

        var userInfos = userIds.Any() 
            ? await userRepository.GetUsersInfoAsync(userIds) 
            : new Dictionary<int, ChatMemberInfo>();
        
        foreach (var notification in mappedNotifications)
        {
            switch (notification.ResourceType)
            {
                case "Auction" or "Product" when notification.ListingId.HasValue:
                    notification.NotificationImageUrl = listingUrls.GetValueOrDefault(notification.ListingId.Value);
                    break;
                case "ChatPage" or "User" when notification.UserId.HasValue:
                    var userInfo = userInfos.GetValueOrDefault(notification.UserId.Value);
                    if (userInfo != null)
                    {
                        notification.NotificationImageUrl = userInfo.PhotoUrl;
                        notification.UserInfo = userInfo;
                    }
                    break;
            }
        }
        
        var readDate = DateTime.UtcNow;
        
        bool anyUpdated = false;
        
        foreach (var notification in notifications)
        {
            var userNotification = notification.NotificationUsers
                .First(nu => nu.UserId == request.UserId);
            
            if (userNotification.ReadAt == null)
            {
                userNotification.ReadAt = readDate;
                notificationRepository.UpdateNotificationUser(userNotification);
                anyUpdated = true;
            }
        }

        if (anyUpdated)
        {
            await unitOfWork.SaveChangesAsync();
        }
        
        return new PageResponse<NotificationInfo>(mappedNotifications, totalItems, request.Query.PageSize, request.Query.PageNumber);
    }
}