using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Repositories;

public class ListingViewRepository(ItemiteDbContext dbContext) : ILIstingViewRepository
{
    public async Task AddAsync(ListingView listingView)
    {
        await dbContext.ListingViews.AddAsync(listingView);
    }
}