using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task CreateCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(Category category);
    Task<List<Category>> GetAllCategories();
    Task<List<Category>> GetMainCategories();
    Task<List<Category>> GetSubCategories(int parentCategoryId);
    Task<List<Category>> GetCategoriesByRootIdAsync(int rootCategoryId);
    Task<Category> GetByNameAsync(string name);
    Task<Category> GetByIdAsync(int parentId);
    Task<bool> CategoryExistsById(int categoryId);
    Task<bool> CategoryExistsByName(string name);
    Task<bool> CategoryExistsByNameExcludingId(string name, int excludeId);
    Task<bool> CategoryExistsByNameInTree(string name, int rootCategoryId);
    Task<bool> CategoryExistsByNameInTreeExcludingId(string name, int rootCategoryId, int excludeId);
    Task<bool> RootCategoryExistsByName(string name);
    Task<bool> RootCategoryExistsByNameExcludingId(string name, int excludeId);
    Task<bool> IsParentCategory(int categoryId);
}