using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationRepository(ItemiteDbContext dbContext) : INotificationRepository
{
    public async Task AddNotification(Notification notification)
    {
        await dbContext.AddAsync(notification);
    }

    public async Task AddNotificationUser(NotificationUser notificationUser)
    {
        await dbContext.AddAsync(notificationUser);
    }

    public IQueryable<Notification> GetUserNotificationsQueryable(int userId)
    {
        return dbContext.Notifications
            .OrderByDescending(n => n.NotificationSent)
            .Include(n => n.NotificationUsers)
            .Where(n => n.NotificationUsers.Any(nu => nu.UserId == userId));
    }

    public async Task<List<NotificationUser>> GetUserNotifications(int userId)
    {
        return await dbContext.NotificationUsers
            .Where(n => n.UserId == userId).ToListAsync();
    }

    public Task<int> GetNotificationUserCount(int notificationId)
    {
        return dbContext.NotificationUsers.CountAsync(n => n.NotificationId == notificationId);
    }

    public async Task<Notification?> GetNotification(int notificationId)
    {
        return await dbContext.Notifications
            .Include(n => n.NotificationUsers)
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<int> GetUserUnreadNotificationsCount(int userId)
    {
        return await dbContext.Notifications
            .Include(n => n.NotificationUsers)
            .Where(n => n.NotificationUsers.Any(nu => nu.UserId == userId && nu.ReadAt == null))
            .CountAsync();
    }
    
    public void UpdateNotification(Notification notification)
    {
        dbContext.Update(notification);
    }

    public void UpdateNotificationUser(NotificationUser notificationUser)
    {
        dbContext.Update(notificationUser);
    }

    public void DeleteNotification(Notification notification)
    {
        dbContext.Remove(notification);
    }

    public void DeleteNotificationUser(NotificationUser notificationUser)
    {
        dbContext.Remove(notificationUser);
    }
}