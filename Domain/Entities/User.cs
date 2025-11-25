using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>
{
    public Location? Location { get; set; }
    public DateTime? EmailConfirmationTokenExpirationDate { get; set; }
    public string? PendingNewEmail { get; set; }
    public DateTime? EmailChangeTokenExpirationDate { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public string? StripeConnectAccountId { get; set; } = null;

    public int? ProfilePhotoId { get; set; }
    public Photo? ProfilePhoto { get; set; }

    public int? BackgroundPhotoId { get; set; }
    public Photo? BackgroundPhoto { get; set; }

    public ICollection<ListingBase> OwnedListings { get; set; } = [];
    public ICollection<AuctionBid> Bids { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Message> ReceivedMessages { get; set; } = [];
    public ICollection<FollowedListing> FollowedListings { get; set; } = [];
    public ICollection<ListingView> ViewedListings { get; set; } = [];
}