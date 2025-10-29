using Domain.DTOs.User;

namespace Domain.DTOs.AuctionListing;

public class AuctionBidResponse
{
    public int Id { get; set; }
    public UserResponse Bidder { get; set; }
    public decimal BidPrice { get; set; }
    public DateTime BidDate { get; set; }
}