using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IBannerRepository
{
    public Task AddAsync(Banner banner);
    public void Update(Banner banner);
    void Remove(Banner banner);
    public Task RemoveAsync(int bannerId);
    Task<Banner?> FindByIdAsync(int bannerId);
    public Task ToggleActive(Banner banner);
    public Task<List<Banner>> FindAllActiveAsync();
    public Task<List<Banner>> FindAllAsync();
}