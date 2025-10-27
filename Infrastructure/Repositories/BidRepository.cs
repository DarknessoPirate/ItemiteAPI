using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BidRepository(ItemiteDbContext dbContext) : IBidRepository
{
    public async Task CreateBid(AuctionBid bid)
    {
        await dbContext.AuctionBids.AddAsync(bid);
    }

    public async Task<List<AuctionBid>> GetAuctionBids(int auctionId)
    {
        return await dbContext.AuctionBids.Where(b => b.AuctionId == auctionId).ToListAsync();
    }
}