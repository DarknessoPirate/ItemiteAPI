using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IBannerRepository
{
    public Task AddAsync(Banner banner);
    public void Update(Banner banner);
    public void Delete(int bannerId);
    public Task ToggleActive(int bannerId);
    public Task<List<Banner>> GetActiveBanners();
    public Task<List<Banner>> GetAllBanners();
}