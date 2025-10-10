using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IListingRepository<T> where T : ListingBase
{
    Task<List<T>> GetAllListingsAsync();
    IQueryable<ProductListing> GetProductListingsQueryable();
    Task CreateListingAsync(T listing);
}