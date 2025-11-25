using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ListingViewRepository(ItemiteDbContext dbContext) : ILIstingViewRepository
{
    public async Task AddAsync(ListingView listingView)
    {
        await dbContext.ListingViews.AddAsync(listingView);
    }

    public async Task<List<ListingView>> GetUserListingViewsAsync(int? userId)
    {
        return await dbContext.ListingViews.Where(lv => lv.UserId == userId).ToListAsync();
    }

    public async Task<List<ListingView>> GetExpiredListingViewsAsync(DateTime expirationDate)
    {
        return await dbContext.ListingViews
            .Where(lv => lv.ViewedAt < expirationDate)
            .ToListAsync();
    }

    public void RemoveRange(IEnumerable<ListingView> listingViews)
    {
        dbContext.ListingViews.RemoveRange(listingViews);
    }
}