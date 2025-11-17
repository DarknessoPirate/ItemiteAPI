using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Notifications.GetUserNotifications;

public class GetUserNotificationsHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetUserNotificationsQuery, List<NotificationInfo>>
{
    public async Task<List<NotificationInfo>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notificationsFromCache = await cacheService.GetAsync<List<NotificationInfo>>($"{CacheKeys.NOTIFICATIONS}{request.UserId}");
        if (notificationsFromCache != null)
        {
            return notificationsFromCache;
        }
        
        var notifications = await notificationRepository.GetUserNotifications(request.UserId);
        
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
        
        var mappedNotifications = mapper.Map<List<NotificationInfo>>(notifications, opt =>
        {
            opt.Items["UserId"] = request.UserId;
        });
        
        await cacheService.SetAsync($"{CacheKeys.NOTIFICATIONS}{request.UserId}", mappedNotifications);
        
        return mappedNotifications;
    }
}