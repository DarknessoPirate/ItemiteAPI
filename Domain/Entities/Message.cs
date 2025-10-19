namespace Domain.Entities;

public class Message
{
   public int Id { get; set; }
   public string? Content { get; set; }
   public DateTime DateSent { get; set; } = DateTime.UtcNow;
   public DateTime? DateModified { get; set; }
   public User Sender { get; set; }
   public int SenderId { get; set; }
   public User Recipient { get; set; }
   public int RecipientId { get; set; }

   public ICollection<MessagePhoto> MessagePhotos { get; set; } = [];
}