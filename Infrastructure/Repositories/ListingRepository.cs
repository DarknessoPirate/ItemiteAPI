using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ListingRepository<T>(ItemiteDbContext dbContext) : IListingRepository<T> where T : ListingBase
{
    public async Task<List<T>> GetAllListingsAsync()
    {
        var listings = await dbContext.Set<T>().ToListAsync();
        return listings;
    }

    public IQueryable<ProductListing> GetProductListingsQueryable()
    {
       return dbContext.Set<ProductListing>().Include(p => p.Categories);
    }

    public async Task CreateListingAsync(T listing)
    {
        await dbContext.Set<T>().AddAsync(listing);
    }
}