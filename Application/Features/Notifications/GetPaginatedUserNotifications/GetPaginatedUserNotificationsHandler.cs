using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Notifications.GetPaginatedUserNotifications;

public class GetPaginatedUserNotificationsHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
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