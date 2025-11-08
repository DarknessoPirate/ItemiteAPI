using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IListingRepository<T> where T : ListingBase
{
    Task<List<T>> GetAllListingsAsync();
    Task<List<T>> GetUserListingsAsync(int userId);
    IQueryable<T> GetListingsQueryable();
    Task<T?> GetListingWithPhotosByIdAsync(int listingId);
    Task<T?> GetListingByIdAsync(int listingId);
    Task CreateListingAsync(T listing);
    Task<List<User>> GetListingFollowersAsync(int listingId);
    Task<List<FollowedListing>> GetUserFollowedListingsAsync(int userId);
    IQueryable<ListingBase> GetListingsFollowedByUserQueryable(int userId);
    Task AddListingToFollowedAsync(FollowedListing followedListing);
    Task<bool> ListingExistsAsync(int listingId);
    void UpdateListing(T listing);
    void DeleteListing(T listing);
    Task<List<T>> GetExpiredFeaturedListingsAsync(DateTime expirationDate);
}