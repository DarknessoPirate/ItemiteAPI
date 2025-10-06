using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository(ItemiteDbContext dbContext) : ICategoryRepository
{
    public async Task CreateCategory(Category category)
    {
        await dbContext.Categories.AddAsync(category);
    }

    public void UpdateCategory(Category category)
    {
        dbContext.Categories.Update(category);
    }

    public void DeleteCategory(Category category)
    {
        dbContext.Categories.Remove(category);
    }

    public async Task<List<Category>> GetAllCategories()
    {
        var categories = await dbContext.Categories.ToListAsync();

        return categories;
    }

    public async Task<List<Category>> GetMainCategories()
    {
        var mainCategories =
            await dbContext.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();

        return mainCategories;
    }

    public async Task<List<Category>> GetSubCategories(int parentCategoryId)
    {
        if (!await CategoryExistsById(parentCategoryId))
            throw new NotFoundException($"Parent category with id: {parentCategoryId} not found");

        var subCategories =
            await dbContext.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .ToListAsync();

        return subCategories;
    }

    public async Task<List<Category>> GetCategoriesByRootIdAsync(int rootCategoryId)
    {
        var categories = await dbContext.Categories.Where(c => c.RootCategoryId == rootCategoryId).ToListAsync();

        return categories;
    }

    public async Task<Category> GetByNameAsync(string name)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Name == name);

        if (category == null)
            throw new NotFoundException($"Category '{name}' not found");

        return category;
    }

    public async Task<Category> GetByIdAsync(int categoryId)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);

        if (category == null)
            throw new NotFoundException($"Category with id: {categoryId} not found");

        return category;
    }

    public async Task<bool> CategoryExistsById(int categoryId)
    {
        var exists = await dbContext.Categories.AnyAsync(x => x.Id == categoryId);

        return exists;
    }

    public async Task<bool> CategoryExistsByName(string name)
    {
        var exists = await dbContext.Categories.AnyAsync(x => x.Name == name);

        return exists;
    }

    public async Task<bool> CategoryExistsByNameExcludingId(string name, int excludeId)
    {
        var exists = await dbContext.Categories
            .AnyAsync(x => x.Name == name && x.Id != excludeId);

        return exists;
    }

    public async Task<bool> IsParentCategory(int categoryId)
    {
        var isParent = await dbContext.Categories.AnyAsync(x => x.ParentCategoryId == categoryId);

        return isParent;
    }
}