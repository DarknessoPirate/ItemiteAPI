namespace Domain.DTOs.Messages;

public class LastMessageResponse
{
    public int MessageId { get; set; }
    public string UserName { get; set; }
    public string? Content { get; set; }
    public DateTime DateSent { get; set; }
}