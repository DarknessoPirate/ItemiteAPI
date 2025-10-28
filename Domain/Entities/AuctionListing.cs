using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class AuctionListing : ListingBase
{
    [Required]
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
    public int? HighestBidId {get; set; } 
    public ICollection<AuctionBid> Bids { get; set; } = new List<AuctionBid>();
}