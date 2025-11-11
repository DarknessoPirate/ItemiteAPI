using MediatR;

namespace Application.Features.Listings.Shared.HighlightListing;

public class HighlightListingCommand : IRequest<string>
{
    public List<int> ListingIds { get; set; }
    public int UserId { get; set; }
}