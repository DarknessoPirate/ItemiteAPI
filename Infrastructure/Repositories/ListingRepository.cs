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

    public async Task<List<T>> GetUserListingsAsync(int userId)
    {
        var userListings = await dbContext.Set<T>()
            .Where(l => l.OwnerId == userId && !l.IsArchived)
            .ToListAsync();
        return userListings;
    }

    public IQueryable<T> GetListingsQueryable()
    {
       return dbContext.Set<T>()
           .Where(l => !l.IsArchived)
           .Include(p => p.Categories)
           .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo);
    }

    public async Task<T?> GetListingByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().Include(p => p.Categories).ThenInclude(c => c.Photo)
            .Include(p => p.Owner).ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo).FirstOrDefaultAsync(l => l.Id == listingId);
        return listing;
    }
    
    public async Task<T?> GetListingWithPhotosByIdAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().Include(p => p.ListingPhotos).ThenInclude(l => l.Photo).FirstOrDefaultAsync(l => l.Id == listingId);
        return listing;
    }

    public async Task CreateListingAsync(T listing)
    {
        await dbContext.Set<T>().AddAsync(listing);
    }

    public async Task<List<User>> GetListingFollowersAsync(int listingId)
    {
        var followers = await dbContext.FollowedListings.Where(f => f.ListingId == listingId).Select(f => f.User)
            .ToListAsync();
        return followers;
    }

    public async Task<List<FollowedListing>> GetUserFollowedListingsAsync(int userId)
    {
        var followedListing = await dbContext.FollowedListings.Where(f => f.UserId == userId).ToListAsync();
        return followedListing;
    }

    public IQueryable<ListingBase> GetListingsFollowedByUserQueryable(int userId)
    {
        return dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.FollowedListings)
            .ThenInclude(f => f.Listing).ThenInclude(l => l.Categories)
            .Include(u => u.FollowedListings)
            .ThenInclude(f => f.Listing).ThenInclude(l => l.ListingPhotos).ThenInclude(lp => lp.Photo)
            .SelectMany(u => u.FollowedListings.OrderByDescending(f => f.FollowedAt))
            .Select(f => f.Listing);
    }
    
    public IQueryable<ListingBase> GetUserListingsQueryable(int userId, bool? areArchived)
    {
        var listingsQuery = dbContext.Set<T>().Include(p => p.Categories)
            .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo);

        if (areArchived != null)
        {
            return listingsQuery.Where(l => l.OwnerId == userId && areArchived == false ? !l.IsArchived : l.IsArchived)
                .OrderByDescending(l => l.DateCreated);
        }
        return listingsQuery.Where(l => l.OwnerId == userId)
            .OrderByDescending(l => l.DateCreated);
            
    }


    public async Task AddListingToFollowedAsync(FollowedListing followedListing)
    {
        await dbContext.FollowedListings.AddAsync(followedListing);
    }

    public void UnfollowListing(FollowedListing followedListing)
    {
        dbContext.FollowedListings.Remove(followedListing);
    }

    public async Task<bool> ListingExistsAsync(int listingId)
    {
        var listing = await dbContext.Set<T>().FirstOrDefaultAsync(l => l.Id == listingId);
        return listing != null;
    }

    public void UpdateListing(T listing)
    {
        dbContext.Set<T>().Update(listing);
    }

    public void DeleteListing(T listing)
    {
        dbContext.Set<T>().Remove(listing);
    }

    public async Task<List<T>> GetExpiredFeaturedListingsAsync(DateTime expirationDate)
    {
        var expiredListings = await dbContext.Set<T>()
            .Include(p => p.ListingPhotos).ThenInclude(l => l.Photo)
            .Where(l => l.IsFeatured == true && l.FeaturedAt < expirationDate)
            .ToListAsync();
    
        return expiredListings;
    }

    public async Task<List<T>> GetListingsToArchiveAsync(DateTime currentDate)
    {
        return await dbContext.Set<T>()
            .Where(l => l.DateEnds <= currentDate && !l.IsArchived)
            .ToListAsync();
    }

    public async Task<UserListingPrice?> GetUserListingPriceAsync(int listingId, int userId)
    {
        return await dbContext.UserListingPrices.FirstOrDefaultAsync(p => p.ListingId == listingId && p.UserId == userId);
    }

    public async Task AddUserListingPriceAsync(UserListingPrice userListingPrice)
    {
        await dbContext.UserListingPrices.AddAsync(userListingPrice);
    }

    public void UpdateUserListingPrice(UserListingPrice userListingPrice)
    {
        dbContext.UserListingPrices.Update(userListingPrice);
    }

    public void DeleteUserListingPrice(UserListingPrice userListingPrice)
    {
        dbContext.UserListingPrices.Remove(userListingPrice);
    }

    public async Task<Dictionary<int, string>> GetListingImageUrlsAsync(List<int> listingIds)
    {
        return await dbContext.ListingPhotos
            .Include(lp => lp.Photo)
            .Where(lp => listingIds.Contains(lp.ListingId) && lp.Order == 1)
            .Select(lp => new { lp.ListingId, lp.Photo.Url })
            .ToDictionaryAsync(x => x.ListingId, x => x.Url);
    }
}