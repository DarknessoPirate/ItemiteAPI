using Domain.DTOs.ProductListing;
using MediatR;

namespace Application.Features.Listings.ProductListings.SetUserPrice;

public class SetUserPriceCommand : IRequest
{
    public int OwnerId { get; set; }
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public SetUserPriceRequest Dto { get; set; }
}