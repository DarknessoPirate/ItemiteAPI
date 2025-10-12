using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>
{
    public string? Location { get; set; }
    public DateTime? EmailConfirmationTokenExpirationDate { get; set; }
    public string? PendingNewEmail { get; set; }
    public DateTime? EmailChangeTokenExpirationDate { get; set; }
    public AuthProvider AuthProvider { get; set; }
    
    public int? ProfilePhotoId { get; set; }
    public Photo? ProfilePhoto { get; set; }
    
    public int? BackgroundPhotoId { get; set; }
    public Photo? BackgroundPhoto { get; set; }

    public ICollection<ListingBase> OwnedListings { get; set; } = new List<ListingBase>();
    public ICollection<AuctionListing> HighestBids { get; set; } = new List<AuctionListing>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}