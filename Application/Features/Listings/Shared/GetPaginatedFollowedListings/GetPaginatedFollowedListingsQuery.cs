using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Listings.Shared.GetPaginatedFollowedListings;

public class GetPaginatedFollowedListingsQuery : IRequest<PageResponse<ListingBasicResponse>>
{
    public PaginateFollowedListingsQuery Query {get; set;}
    public int UserId { get; set; }
}