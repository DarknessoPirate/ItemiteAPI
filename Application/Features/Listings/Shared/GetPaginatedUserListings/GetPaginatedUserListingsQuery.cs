using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Listings.Shared.GetPaginatedUserListings;

public class GetPaginatedUserListingsQuery : IRequest<PageResponse<ListingBasicResponse>>
{
    public PaginateUserListingsQuery Query {get; set;}
    public int UserId { get; set; }
    public int? CurrentUserId {get; set;}
}