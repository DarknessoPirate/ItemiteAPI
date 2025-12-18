using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task RemoveAsync(int bannerId)
    {
        var banner = await context.Banners.FindAsync(bannerId);

        if (banner != null)
            context.Banners.Remove(banner);
    }
    public void Remove(Banner banner)
    {
        context.Banners.Remove(banner);
    }

    public async Task<Banner?> FindByIdAsync(int bannerId)
    {
        return await context.Banners.FindAsync(bannerId);
    }

    public Task ToggleActive(Banner banner)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Banner>> FindAllActiveAsync()
    {
        return await context.Banners
        .Where(b => b.IsActive)
        .ToListAsync();
    }

    public async Task<List<Banner>> FindAllAsync()
    {
        return await context.Banners.ToListAsync();
    }
}