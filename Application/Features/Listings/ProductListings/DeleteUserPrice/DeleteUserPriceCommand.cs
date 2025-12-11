using MediatR;

namespace Application.Features.Listings.ProductListings.DeleteUserPrice;

public class DeleteUserPriceCommand : IRequest
{
    public int OwnerId { get; set; }
    public int UserId { get; set; }
    public int ListingId { get; set; }
}