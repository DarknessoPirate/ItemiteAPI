using AutoMapper;
using Domain.Configs;
using Domain.DTOs.AuctionListing;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Listings.AuctionListings.GetBidHistory;

public class GetBidHistoryHandler(
    IBidRepository bidRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<GetBidHistoryQuery, List<AuctionBidResponse>>
{
    public async Task<List<AuctionBidResponse>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
    {
        var cachedBids = await cacheService.GetAsync<List<AuctionBidResponse>>($"{CacheKeys.BIDS}{request.AuctionId}");
        if (cachedBids != null)
        {
            return cachedBids;
        }
        
        var bids = await bidRepository.GetAuctionBids(request.AuctionId);
        var mappedBids = mapper.Map<List<AuctionBidResponse>>(bids);
        await cacheService.SetAsync($"{CacheKeys.BIDS}{request.AuctionId}", mappedBids);
        return mappedBids;
    }
}