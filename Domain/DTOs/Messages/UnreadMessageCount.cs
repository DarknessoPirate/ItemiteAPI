namespace Domain.DTOs.Messages;

public class UnreadMessageCount
{
    public int UserId { get; set; }
    public int OtherUserId { get; set; }
    public int ListingId { get; set; }
    public int Count { get; set; }
}