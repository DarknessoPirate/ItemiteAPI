using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface ILIstingViewRepository
{
    public Task AddAsync(ListingView listingView);
    Task<List<ListingView>> GetUserListingViewsAsync(int? userId);
}