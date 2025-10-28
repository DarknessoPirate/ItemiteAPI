using Domain.DTOs.AuctionListing;
using MediatR;

namespace Application.Features.Listings.AuctionListings.PlaceBid;

public class PlaceBidCommand : IRequest<int>
{
    public PlaceBidRequest BidDto { get; set; }
    public int AuctionId { get; set; }
    public int UserId { get; set; }
}