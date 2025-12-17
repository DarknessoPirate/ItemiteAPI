using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
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
            .Where(n => n.ResourceType is ResourceType.Auction or ResourceType.Product && n.ListingId.HasValue)
            .Select(n => n.ListingId!.Value)
            .Distinct()
            .ToList();

        var userIds = mappedNotifications
            .Where(n => n.ResourceType is ResourceType.ChatPage or ResourceType.User && n.UserId.HasValue)
            .Select(n => n.UserId!.Value)
            .Distinct()
            .ToList();
        
        var listingUrls = listingIds.Any() 
            ? await listingRepository.GetListingImageUrlsAsync(listingIds) 
            : new Dictionary<int, string>();

        var userUrls = userIds.Any() 
            ? await userRepository.GetUserProfilePhotoUrlsAsync(userIds) 
            : new Dictionary<int, string?>();
        
        foreach (var notification in mappedNotifications)
        {
            notification.NotificationImageUrl = notification.ResourceType switch
            {
                ResourceType.Auction or ResourceType.Product when notification.ListingId.HasValue 
                    => listingUrls.GetValueOrDefault(notification.ListingId.Value),
                ResourceType.ChatPage or ResourceType.User when notification.UserId.HasValue 
                    => userUrls.GetValueOrDefault(notification.UserId.Value),
                _ => null
            };
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