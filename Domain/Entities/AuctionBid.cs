using Domain.Enums;

namespace Domain.Entities;

public class AuctionBid
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public decimal BidPrice { get; set; }
    public DateTime BidDate { get; set; } = DateTime.UtcNow;
    
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    
    public User Bidder { get; set; }
    public AuctionListing Auction { get; set; }
}