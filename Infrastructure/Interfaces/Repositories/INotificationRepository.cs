using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddNotification(Notification notification);
    Task AddNotificationUser(NotificationUser notificationUser);
    IQueryable<Notification> GetUserNotificationsQueryable(int userId);
    Task<List<NotificationUser>> GetUserNotifications(int userId);
    Task<int> GetNotificationUserCount(int notificationId);
    Task<Notification?> GetNotification(int notificationId);
    Task<int> GetUserUnreadNotificationsCount(int userId);
    void UpdateNotification(Notification notification);
    void UpdateNotificationUser(NotificationUser notificationUser);
    void DeleteNotification(Notification notification);
    void DeleteNotificationUser(NotificationUser notificationUser);
}