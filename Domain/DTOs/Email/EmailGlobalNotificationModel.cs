namespace Domain.DTOs.Email;

public class EmailGlobalNotificationModel
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RecipientUsername { get; set; } = string.Empty;
}