using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Message
{
    public int Id { get; set; }
    [MaxLength(1000)] 
    public string? Content { get; set; } = null;
    public DateTime DateSent { get; set; } = DateTime.UtcNow;
    public DateTime? DateModified { get; set; } = null;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public User Sender { get; set; }
    public int SenderId { get; set; }
    public User Recipient { get; set; }
    public int RecipientId { get; set; }
    public ListingBase Listing { get; set; }
    public int ListingId { get; set; }

    public ICollection<MessagePhoto> MessagePhotos { get; set; } = [];
}