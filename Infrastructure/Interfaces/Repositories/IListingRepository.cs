using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IListingRepository<T> where T : ListingBase
{
    Task<List<T>> GetAllListingsAsync();
    IQueryable<T> GetListingsQueryable();
    Task<T?> GetListingWithPhotosByIdAsync(int listingId);
    Task<T?> GetListingByIdAsync(int listingId);
    Task CreateListingAsync(T listing);
    void UpdateListing(T listing);
    void DeleteListing(T listing);
}