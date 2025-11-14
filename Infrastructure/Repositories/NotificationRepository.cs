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
            .Where(n => n.NotificationUsers.Any(u => u.UserId == userId) || n.ToEveryUser)
            .ToListAsync();
    }

    public async Task<List<NotificationUser>> GetUserNotificationUsers(int userId)
    {
        return await dbContext.NotificationUsers.Where(n => n.UserId == userId).ToListAsync();
    }

    public void UpdateNotification(Notification notification)
    {
        dbContext.Update(notification);
    }

    public void UpdateNotificationUser(NotificationUser notificationUser)
    {
        dbContext.Update(notificationUser);
    }
}