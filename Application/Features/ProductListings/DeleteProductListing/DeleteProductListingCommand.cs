using MediatR;

namespace Application.Features.ProductListings.DeleteProductListing;

public class DeleteProductListingCommand : IRequest
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
}