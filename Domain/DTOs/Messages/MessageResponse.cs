using Domain.DTOs.Photo;

namespace Domain.DTOs.Messages;

public class MessageResponse
{
    public int MessageId { get; set; }
    public string? Content { get; set; }
    public DateTime DateSent { get; set; }
    public DateTime? DateModified { get; set; }
    public bool IsRead { get; set; }
    public DateTime? DateRead { get; set; }
    public int SenderId { get; set; }
    public int ListingId { get; set; }
    public int RecipientId { get; set; }
    public bool IsDeleted { get; set; }
    public List<PhotoResponse> Photos { get; set; } = [];
}