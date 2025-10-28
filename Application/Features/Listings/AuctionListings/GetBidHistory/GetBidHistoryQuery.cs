using Domain.DTOs.AuctionListing;
using MediatR;

namespace Application.Features.Listings.AuctionListings.GetBidHistory;

public class GetBidHistoryQuery : IRequest<List<AuctionBidResponse>>
{
    public int AuctionId { get; set; }
}