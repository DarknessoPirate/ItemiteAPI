using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Listings.Shared.GetPaginatedUserListings;

public class GetPaginatedUserListingsHandler(
    IListingRepository<ListingBase> listingRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetPaginatedUserListingsQuery, PageResponse<ListingBasicResponse>>
{
    public async Task<PageResponse<ListingBasicResponse>> Handle(GetPaginatedUserListingsQuery request, CancellationToken cancellationToken)
    {
        var cachedListings =
            await cacheService.GetAsync<PageResponse<ListingBasicResponse>>($"{CacheKeys.LISTINGS}{request.UserId}_{request.Query}");
        if (cachedListings != null)
        {
            if (request.CurrentUserId != null)
            {
                cachedListings.Items = await SetIsFollowedField(cachedListings.Items, request.CurrentUserId.Value);
            }
            return cachedListings;
        }

        var queryable = listingRepository.GetUserListingsQueryable(request.UserId, request.Query.AreArchived);
        
        int totalItems = await queryable.CountAsync(cancellationToken);
        
        queryable = queryable
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize);

        var userListings = await queryable.ToListAsync(cancellationToken);
        var mappedListings = mapper.Map<List<ListingBasicResponse>>(userListings);
        
        var pageResponse = new PageResponse<ListingBasicResponse>(mappedListings, totalItems, request.Query.PageSize, request.Query.PageNumber);

        await cacheService.SetAsync($"{CacheKeys.LISTINGS}{request.UserId}_{request.Query}", pageResponse, 5);
        
        if (request.CurrentUserId != null)
        {
            pageResponse.Items = await SetIsFollowedField(pageResponse.Items, request.CurrentUserId.Value);
        }
        
        return pageResponse;
    }
    
    private async Task<List<ListingBasicResponse>> SetIsFollowedField(List<ListingBasicResponse> responses, int userId)
    {
        var followedListings = await listingRepository.GetUserFollowedListingsAsync(userId);
        foreach (var response in responses)
        {
            response.IsFollowed = followedListings.Select(f => f.ListingId).Contains(response.Id);
        }
        return responses;
    }
}