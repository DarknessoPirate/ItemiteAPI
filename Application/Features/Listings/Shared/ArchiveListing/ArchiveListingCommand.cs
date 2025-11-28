using MediatR;

namespace Application.Features.Listings.Shared.ArchiveListing;

public class ArchiveListingCommand : IRequest
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
}