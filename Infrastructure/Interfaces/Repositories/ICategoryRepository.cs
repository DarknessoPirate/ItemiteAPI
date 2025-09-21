using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task CreateCategory(Category category);
    Task<List<Category>> GetCategories();
    Task<Category> GetByNameAsync(string name);
    Task<Category> GetByIdAsync(int parentId);
    Task<bool> CategoryExistsById(int categoryId);
    Task<bool> CategoryExistsByName(string name);
}