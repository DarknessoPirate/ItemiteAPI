using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.Messages;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NotificationService(
    IHubContext<BroadcastHub> broadcastHub,
    IHubContext<NotificationHub> notificationHub,
    ILogger<NotificationService> logger,
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cacheService
) : INotificationService
{
    public async Task NotifyMessageReceived(int recipientId, MessageResponse message)
    {
        await notificationHub.Clients
            .User(recipientId.ToString())
            .SendAsync("MessageReceived", message);
    }

    public async Task NotifyMessageUpdated(int recipientId, MessageResponse updatedMessage)
    {
        await notificationHub.Clients
            .User(recipientId.ToString())
            .SendAsync("MessageUpdated", updatedMessage);
        
    }

    public async Task NotifyMessageDeleted(int recipientId, int messageId, string messageDeletedString)
    {
        var notification = new
        {
            messageId = messageId,
            messageDeletedString = messageDeletedString
        };
        await notificationHub.Clients
            .User(recipientId.ToString())
            .SendAsync("MessageDeleted", notification);
    }

    public async Task NotifyListingUpdated(List<int> userIds, ListingBasicInfo listingInfo)
    {
        await notificationHub.Clients.Users(userIds.Select(id => id.ToString()))
            .SendAsync("ListingUpdated", listingInfo);
    }

    public async Task SendNotification(List<int> userIds, int senderId, NotificationInfo notificationInfo)
    {
        if (userIds.Count == 0)
        {
            logger.LogInformation("Notification has not been created - recipients count is 0");
            return;
        }
        var notificationEntity = mapper.Map<Notification>(notificationInfo);
        var recipientIds = userIds.Where(id => id != senderId).ToList();

        await unitOfWork.BeginTransactionAsync();
        try
        {
            await notificationRepository.AddNotification(notificationEntity);
            await unitOfWork.SaveChangesAsync();

            foreach (var userId in recipientIds)
            {
                var notificationUser = new NotificationUser
                {
                    UserId = userId,
                    NotificationId = notificationEntity.Id
                };
                await notificationRepository.AddNotificationUser(notificationUser);

                await cacheService.RemoveByPatternAsync($"{CacheKeys.NOTIFICATIONS}{userId}*");
            }

            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError($"Error when sending a notification: {ex.Message}");
            throw;
        }
        
        await notificationHub.Clients.Users(recipientIds.Select(i => i.ToString()))
            .SendAsync("Notification", notificationInfo);
        
        logger.LogInformation($"Notification sent for users: {string.Join(",", userIds)}" );
        logger.LogInformation($"Notification content: {notificationInfo.Message}" );
        logger.LogInformation($"Notification url: {notificationInfo.ResourceType.ToString()}" );
    }
    
    public async Task SendNotificationsBatch(Dictionary<int, NotificationInfo> userNotifications)
    {
        if (userNotifications.Count == 0)
        {
            logger.LogInformation("Notifications batch is empty");
            return;
        }
    
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var notificationEntities = new List<Notification>();
            
            foreach (var (userId, notificationInfo) in userNotifications)
            {
                var notificationEntity = mapper.Map<Notification>(notificationInfo);
                await notificationRepository.AddNotification(notificationEntity);
                notificationEntities.Add(notificationEntity);
            }
            
            await unitOfWork.SaveChangesAsync();
            
            for (int i = 0; i < notificationEntities.Count; i++)
            {
                var notification = notificationEntities[i];
                var userId = userNotifications.Keys.ElementAt(i);
                
                var notificationUser = new NotificationUser
                {
                    UserId = userId,
                    NotificationId = notification.Id
                };
                await notificationRepository.AddNotificationUser(notificationUser);
            }
            
            await unitOfWork.CommitTransactionAsync(); 
            
            foreach (var userId in userNotifications.Keys)
            {
                await cacheService.RemoveByPatternAsync($"{CacheKeys.NOTIFICATIONS}{userId}*");
            }
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError($"Error when sending notifications batch: {ex.Message}");
            throw;
        }
        
        foreach (var (userId, notificationInfo) in userNotifications)
        {
            await notificationHub.Clients.User(userId.ToString())
                .SendAsync("Notification", notificationInfo);
        }
        
        logger.LogInformation($"Batch notifications sent for {userNotifications.Count} users");
    }
}