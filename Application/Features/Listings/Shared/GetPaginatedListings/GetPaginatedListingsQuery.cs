using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using Domain.Enums;
using MediatR;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsQuery : IRequest<PageResponse<ListingBasicResponse>>
{
    public PaginateListingQuery Query {get; set;}
    public int? UserId {get; set;}

    public override string ToString()
    {
        return $"{UserId.ToString() ?? "null"}_{Query}";
    }
}