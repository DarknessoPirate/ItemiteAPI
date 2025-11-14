namespace Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public string? NotificationImageUrl { get; set; }
    public string? UrlToResource { get; set; }
    public string Message { get; set; }
    public bool ToEveryUser { get; set; } = false;
    public DateTime NotificationSent { get; set; } = DateTime.UtcNow;
    
    public ICollection<NotificationUser> NotificationUsers { get; set; } = [];
}