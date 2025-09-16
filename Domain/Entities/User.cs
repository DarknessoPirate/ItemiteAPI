namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required bool IsActive { get; set; } = false;
    public required bool IsLocked { get; set; } = false;
    public string? PhoneNumber { get; set; } = null;
    public string? Location { get; set; } = null;
    public string? PhotoUrl { get; set; } = null;
    
    public ICollection<ListingBase>? OwnedListings { get; set; } = new List<ListingBase>();
    public ICollection<AuctionListing>? HighestBids { get; set; } = new List<AuctionListing>();
}