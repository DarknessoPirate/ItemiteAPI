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

    public async Task<List<Category>> GetCategories()
    {
        var categories = await dbContext.Categories.ToListAsync();
        
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
}