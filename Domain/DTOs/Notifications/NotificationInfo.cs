namespace Domain.DTOs.Notifications;

public class NotificationInfo
{
    public int NotificationId { get; set; }
    public string Message { get; set; }
    public string? NotificationImageUrl { get; set; }
    public string? UrlToResource { get; set; }
    public DateTime NotificationSent { get; set; }
    public DateTime? ReadAt { get; set; }
}