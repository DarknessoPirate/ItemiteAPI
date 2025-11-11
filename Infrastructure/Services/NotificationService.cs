using Domain.DTOs.Listing;
using Domain.DTOs.Messages;
using Infrastructure.Interfaces.Services;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NotificationService(
    IHubContext<BroadcastHub> broadcastHub,
    IHubContext<NotificationHub> notificationHub,
    ILogger<NotificationService> logger
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
}