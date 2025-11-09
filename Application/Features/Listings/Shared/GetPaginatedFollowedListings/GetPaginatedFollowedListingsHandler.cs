using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Listings.Shared.GetPaginatedFollowedListings;

public class GetPaginatedFollowedListingsHandler(
    IListingRepository<ListingBase> listingRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetPaginatedFollowedListingsQuery, PageResponse<ListingBasicResponse>>
{
    public async Task<PageResponse<ListingBasicResponse>> Handle(GetPaginatedFollowedListingsQuery request, CancellationToken cancellationToken)
    {
        var cachedListings =
            await cacheService.GetAsync<PageResponse<ListingBasicResponse>>($"{CacheKeys.LISTINGS}{request.UserId}_{request.Query}");
        if (cachedListings != null)
        {
            return cachedListings;
        }

        var queryable = listingRepository.GetListingsFollowedByUserQueryable(request.UserId);
        
        int totalItems = await queryable.CountAsync(cancellationToken);
        
        queryable = queryable
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize);

        var followedListings = await queryable.ToListAsync(cancellationToken);
        var mappedListings = mapper.Map<List<ListingBasicResponse>>(followedListings);
        
        foreach (var listing in mappedListings)
        {
            listing.IsFollowed = true;
        }
        
        var pageResponse = new PageResponse<ListingBasicResponse>(mappedListings, totalItems, request.Query.PageSize, request.Query.PageNumber);
        
        await cacheService.SetAsync($"{CacheKeys.LISTINGS}{request.UserId}_{request.Query}", pageResponse, 5);
        return pageResponse;
    }
}