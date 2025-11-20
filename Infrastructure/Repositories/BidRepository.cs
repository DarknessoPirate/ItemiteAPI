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
        return await dbContext.AuctionBids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId).OrderByDescending(b => b.BidPrice).ToListAsync();
    }

    public async Task<AuctionBid?> GetCurrentHighestBid(int auctionId)
    {
        return await dbContext.AuctionBids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId).OrderByDescending(b => b.BidPrice).FirstOrDefaultAsync();
    }
}