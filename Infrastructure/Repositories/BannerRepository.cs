using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Repositories;

public class BannerRepository(ItemiteDbContext context) : IBannerRepository
{
    public async Task AddAsync(Banner banner)
    {
        await context.Banners.AddAsync(banner);
    }

    public void Update(Banner banner)
    {
        throw new NotImplementedException();
    }

    public void Delete(int bannerId)
    {
        throw new NotImplementedException();
    }

    public Task ToggleActive(int bannerId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Banner>> GetActiveBanners()
    {
        throw new NotImplementedException();
    }

    public Task<List<Banner>> GetAllBanners()
    {
        throw new NotImplementedException();
    }
}