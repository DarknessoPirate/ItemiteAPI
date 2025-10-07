using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>
{
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime? EmailConfirmationTokenExpirationDate { get; set; }
    public AuthProvider AuthProvider { get; set; }

    public ICollection<ListingBase> OwnedListings { get; set; } = new List<ListingBase>();
    public ICollection<AuctionListing> HighestBids { get; set; } = new List<AuctionListing>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}