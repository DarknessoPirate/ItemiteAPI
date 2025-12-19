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

    public async Task<List<AuctionBid>> GetAuctionBidsSortedByPrice(int auctionId)
    {
        return await dbContext.AuctionBids
            .Include(b => b.Bidder)
            .Include(b => b.Payment) // Include payment for checking authorization status
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidPrice)
            .ToListAsync();
    }

    public async Task<AuctionBid?> GetBidByIdAsync(int bidId)
    {
        return await dbContext.AuctionBids
            .Include(b => b.Bidder)
            .Include(b => b.Payment) 
            .Include(b => b.Auction)
            .ThenInclude(a => a.Owner) // seller included for transfer data
            .FirstOrDefaultAsync(b => b.Id == bidId);
    }
}