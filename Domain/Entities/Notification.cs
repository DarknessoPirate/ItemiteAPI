using Domain.Enums;

namespace Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? ListingId { get; set; }
    public ResourceType? ResourceType { get; set; }
    public string Message { get; set; }
    public DateTime NotificationSent { get; set; } = DateTime.UtcNow;
    public ICollection<NotificationUser> NotificationUsers { get; set; } = [];
}