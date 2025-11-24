using Domain.DTOs.Listing;
using Domain.Enums;
using MediatR;

namespace Application.Features.Listings.Shared.GetUserDedicatedListings;

public class GetUserDedicatedListingsQuery : IRequest<List<ListingBasicResponse>>
{
    public int? UserId { get; set; }
    public ListingType? ListingType {get; set;} 
}