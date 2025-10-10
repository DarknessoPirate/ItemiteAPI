using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IListingRepository<T> where T : ListingBase
{
    Task<List<T>> GetAllListingsAsync();
    IQueryable<T> GetListingsQueryable();
    Task CreateListingAsync(T listing);
    void DeleteListing(T listing);
}