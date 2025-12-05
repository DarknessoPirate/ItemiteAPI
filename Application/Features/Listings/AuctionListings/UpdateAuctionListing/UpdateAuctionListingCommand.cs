using Domain.DTOs.AuctionListing;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Listings.AuctionListings.UpdateAuctionListing;

public class UpdateAuctionListingCommand : IRequest<AuctionListingResponse>
{
    public UpdateAuctionListingRequest UpdateDto { get; set; }
    public List<FileWrapperWithOrder> NewImages { get; set; } = [];
    public int ListingId { get; set; }
    public int UserId { get; set; }
}