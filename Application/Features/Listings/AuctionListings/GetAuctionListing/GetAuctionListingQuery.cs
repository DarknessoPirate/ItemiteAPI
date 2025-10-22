using Domain.DTOs.AuctionListing;
using MediatR;

namespace Application.Features.Listings.AuctionListings.GetAuctionListing;

public class GetAuctionListingQuery : IRequest<AuctionListingResponse>
{
    public int ListingId { get; set; }
    public int? UserId { get; set; }
}