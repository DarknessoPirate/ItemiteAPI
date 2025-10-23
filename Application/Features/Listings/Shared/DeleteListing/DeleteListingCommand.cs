using MediatR;

namespace Application.Features.Listings.Shared.DeleteListing;

public class DeleteListingCommand : IRequest
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
}