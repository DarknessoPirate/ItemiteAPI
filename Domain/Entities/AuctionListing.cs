using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class AuctionListing : ListingBase
{
    [Required]
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }

    public int? HighestBidderId { get; set; } = null;
    public User? HighestBidder { get; set; } = null;
}