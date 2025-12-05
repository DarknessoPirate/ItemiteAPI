using Domain.ValueObjects;

namespace Domain.DTOs.AuctionListing;

public class UpdateAuctionListingRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Location? Location { get; set; }
    public int CategoryId { get; set; }
    public decimal? StartingBid { get; set; }

    public List<int> ExistingPhotoIds { get; set; } = [];
    public List<int> ExistingPhotoOrders { get; set; } = [];
}