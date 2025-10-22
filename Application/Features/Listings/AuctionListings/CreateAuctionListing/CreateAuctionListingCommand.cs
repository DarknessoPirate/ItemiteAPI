using Domain.DTOs.AuctionListing;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Listings.AuctionListings.CreateAuctionListing;

public class CreateAuctionListingCommand : IRequest<int>
{
    public CreateAuctionListingRequest AuctionListingDto { get; set; }
    public List<FileWrapper> Images {get; set;}
    public int UserId {get; set;}
}