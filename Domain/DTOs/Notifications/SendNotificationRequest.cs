namespace Domain.DTOs.Notifications;

public class SendNotificationRequest
{
    public string EmailSubject { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
}