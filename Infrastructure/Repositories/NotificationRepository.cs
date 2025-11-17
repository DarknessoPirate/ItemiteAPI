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

    public async Task<List<Notification>> GetUserNotifications(int userId)
    {
        return await dbContext.Notifications
            .OrderByDescending(n => n.NotificationSent)
            .Include(n => n.NotificationUsers)
            .Where(n => n.NotificationUsers.Any(nu => nu.UserId == userId))
            .ToListAsync();
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