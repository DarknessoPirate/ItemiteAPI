using MediatR;

namespace Application.Features.Listings.Shared.UnfollowListing;

public class UnfollowListingCommand : IRequest
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
}