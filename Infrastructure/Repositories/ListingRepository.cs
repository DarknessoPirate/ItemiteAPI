using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Exceptions;
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

    public IQueryable<T> GetListingsQueryableWithCategories()
    {
       return dbContext.Set<T>().Include(p => p.Categories);
    }

    public async Task<T> GetListingWithCategoriesAndOwnerByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().Include(p => p.Categories).Include(p => p.Owner).FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null)
        {
            throw new NotFoundException($"Listing with Id: {listingId} not found");
        }
        return listing;
    }
    
    public async Task<T> GetListingByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null)
        {
            throw new NotFoundException($"Listing with Id: {listingId} not found");
        }
        return listing;
    }

    public async Task CreateListingAsync(T listing)
    {
        await dbContext.Set<T>().AddAsync(listing);
    }

    public void DeleteListing(T listing)
    {
        dbContext.Set<T>().Remove(listing);
    }
}