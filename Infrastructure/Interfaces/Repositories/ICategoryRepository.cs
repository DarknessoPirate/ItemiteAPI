using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task CreateCategory(Category category);
    Task UpdateCategory(Category category);
    Task DeleteCategory(Category category);
    Task<List<Category>> GetAllCategories();
    Task<List<Category>> GetMainCategories();
    Task<List<Category>> GetSubCategories(int parentCategoryId);
    Task<Category> GetByNameAsync(string name);
    Task<Category> GetByIdAsync(int parentId);
    Task<bool> CategoryExistsById(int categoryId);
    Task<bool> CategoryExistsByName(string name);
}