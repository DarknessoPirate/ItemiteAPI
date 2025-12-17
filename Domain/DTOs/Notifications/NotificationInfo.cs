using Domain.Enums;

namespace Domain.DTOs.Notifications;

public class NotificationInfo
{
    public int NotificationId { get; set; }
    public string Message { get; set; }
    public string? NotificationImageUrl { get; set; }
    
    public int? UserId { get; set; }
    public int? ListingId { get; set; }
    public ResourceType? ResourceType { get; set; }
    
    public DateTime NotificationSent { get; set; }
    public DateTime? ReadAt { get; set; }
}