using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IBidRepository
{
    Task CreateBid(AuctionBid bid);
    Task<List<AuctionBid>> GetAuctionBids(int auctionId);
    Task<AuctionBid?> GetCurrentHighestBid(int auctionId);
}