using MediatR;

namespace Application.Features.Listings.Shared.FollowListing;

public class FollowListingCommand : IRequest<int>
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
}