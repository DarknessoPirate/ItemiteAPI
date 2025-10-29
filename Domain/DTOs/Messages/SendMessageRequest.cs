namespace Domain.DTOs.Messages;

public class SendMessageRequest
{
   public string? Content { get; set; }
   public int ListingId { get; set; }
   public int RecipientId { get; set; }
}