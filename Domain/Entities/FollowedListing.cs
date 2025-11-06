namespace Domain.Entities;

public class FollowedListing
{
    public int Id { get; set; }
    
    public int ListingId { get; set; }
    public ListingBase Listing { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    public int RootCategoryId { get; set; }
}