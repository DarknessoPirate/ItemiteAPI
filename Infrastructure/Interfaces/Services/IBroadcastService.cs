using Domain.DTOs.Notifications;

namespace Infrastructure.Interfaces.Services;

public interface IBroadcastService
{
    Task SendMessageToAllUsers(NotificationInfo notificationInfo);
}