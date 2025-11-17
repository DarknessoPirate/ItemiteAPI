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
        logger.LogInformation($"Notification url: {notificationInfo.UrlToResource}" );
    }
    
}