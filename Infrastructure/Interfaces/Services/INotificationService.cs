using Domain.DTOs.Listing;
using Domain.DTOs.Messages;
using Domain.DTOs.Notifications;

namespace Infrastructure.Interfaces.Services;

public interface INotificationService
{
    Task NotifyMessageReceived(int recipientId, MessageResponse message);
    Task NotifyMessageUpdated(int recipientId, MessageResponse updatedMessage);
    Task NotifyMessageDeleted(int recipientId, int messageId, string messageDeletedString);
    Task NotifyListingUpdated(List<int> userIds, ListingBasicInfo listingInfo);
    
    Task SendNotification(List<int> userIds, int senderId, NotificationInfo notificationInfo);
}