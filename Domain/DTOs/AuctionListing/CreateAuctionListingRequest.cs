using Domain.ValueObjects;

namespace Domain.DTOs.AuctionListing;

public class CreateAuctionListingRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Location? Location { get; set; }
    public int CategoryId { get; set; }
    public List<int> ImageOrders { get; set; }
    public decimal StartingBid { get; set; }
    public DateTime? DateEnds { get; set; } 
}