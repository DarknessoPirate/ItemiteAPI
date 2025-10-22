using Domain.DTOs.ProductListing;
using MediatR;

namespace Application.Features.Listings.ProductListings.GetProductListing;

public class GetProductListingQuery : IRequest<ProductListingResponse>
{
    public int ListingId { get; set; }
    public int? UserId { get; set; }
}