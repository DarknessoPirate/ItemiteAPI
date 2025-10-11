using Domain.DTOs.ProductListing;
using MediatR;

namespace Application.Features.ProductListings.UpdateProductListing;

public class UpdateProductListingCommand : IRequest<ProductListingBasicResponse>
{
    public UpdateProductListingRequest UpdateDto { get; set; }
    public int ListingId { get; set; }
    public int UserId { get; set; }
}