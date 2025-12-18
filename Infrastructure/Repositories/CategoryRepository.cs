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
        var categories = await dbContext.Categories.Include(c => c.Photo).ToListAsync();

        return categories;
    }

    public async Task<List<Category>> GetMainCategories()
    {
        var mainCategories =
            await dbContext.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.Photo)
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

    public async Task<List<Category>> GetDescendantsByCategoryId(int parentCategoryId)
    {
        var descendants = new List<Category>();

        // Get direct children
        var directChildren = await dbContext.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .ToListAsync();

        descendants.AddRange(directChildren);

        // Recursively get descendants of each child
        foreach (var child in directChildren)
        {
            var childDescendants = await GetDescendantsByCategoryId(child.Id);
            descendants.AddRange(childDescendants);
        }

        return descendants;
    }

    public async Task<List<Category>> GetAllParentsRelatedToCategory(Category category)
    {
        var relatedParents = new List<Category>();
        
        var parent = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId);
        while (parent != null)
        {
            relatedParents.Add(parent);
            parent = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == parent.ParentCategoryId);
        }
        return relatedParents;
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
        var category = await dbContext.Categories.Include(c => c.Photo).FirstOrDefaultAsync(x => x.Id == categoryId);

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

    public async Task<bool> CategoryExistsByNameInTree(string name, int rootCategoryId)
    {
        var rootCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == rootCategoryId);

        if (rootCategory != null && rootCategory.Name == name)
            return true;

        return await dbContext.Categories.AnyAsync(c => c.Name == name && c.RootCategoryId == rootCategoryId);
    }

    public async Task<bool> CategoryExistsByNameInTreeExcludingId(string name, int rootCategoryId, int excludeId)
    {
        // check if the root category itself has this name (excluding current category)
        var rootCategory = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == rootCategoryId);
        if (rootCategory != null && rootCategory.Name == name && rootCategory.Id != excludeId)
            return true;

        // check if name exists in any subcategories of this tree (excluding current category)
        return await dbContext.Categories
            .AnyAsync(x => x.Name == name && x.RootCategoryId == rootCategoryId && x.Id != excludeId);
    }

    public async Task<bool> RootCategoryExistsByName(string name)
    {
        return await dbContext.Categories
            .AnyAsync(x => x.Name == name && x.ParentCategoryId == null);
    }

    public async Task<bool> RootCategoryExistsByNameExcludingId(string name, int excludeId)
    {
        return await dbContext.Categories
            .AnyAsync(x => x.Name == name && x.ParentCategoryId == null && x.Id != excludeId);
    }

    public async Task<bool> IsParentCategory(int categoryId)
    {
        var isParent = await dbContext.Categories.AnyAsync(x => x.ParentCategoryId == categoryId);

        return isParent;
    }
}