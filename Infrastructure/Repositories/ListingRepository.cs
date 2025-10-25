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

    public IQueryable<T> GetListingsQueryable()
    {
       return dbContext.Set<T>().Include(p => p.Categories)
           .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo);
    }

    public async Task<T> GetListingByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().Include(p => p.Categories)
            .Include(p => p.Owner).ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo).FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null)
        {
            throw new NotFoundException($"{typeof(T).Name} with Id: {listingId} not found");
        }
        return listing;
    }
    
    public async Task<T> GetListingWithPhotosByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().Include(p => p.ListingPhotos).ThenInclude(l => l.Photo).FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null)
        {
            throw new NotFoundException($"{typeof(T).Name} with Id: {listingId} not found");
        }
        return listing;
    }

    public async Task CreateListingAsync(T listing)
    {
        await dbContext.Set<T>().AddAsync(listing);
    }

    public void UpdateListing(T listing)
    {
        dbContext.Set<T>().Update(listing);
    }

    public void DeleteListing(T listing)
    {
        dbContext.Set<T>().Remove(listing);
    }
}