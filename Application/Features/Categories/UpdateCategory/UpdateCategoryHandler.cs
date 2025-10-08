using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Category;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Application.Features.Categories.UpdateCategory;

public class UpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ILogger<UpdateCategoryHandler> logger,
    IUnitOfWork unitOfWork,
    ICacheService cache
) : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryToUpdate = await categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryToUpdate == null)
            throw new NotFoundException($"Category with id {request.CategoryId} not found");

        Category? parentCategory = null;

        // check if modifying to or was root category
        if (request.dto.ParentCategoryId == null)
        {
            var rootExists =
                await categoryRepository.RootCategoryExistsByNameExcludingId(request.dto.Name, request.CategoryId);
            if (rootExists)
                throw new BadRequestException($"A root category with name '{request.dto.Name}' already exists");
        }
        else
        {
            // check if parent exists
            parentCategory = await categoryRepository.GetByIdAsync(request.dto.ParentCategoryId.Value);
            if (parentCategory == null)
                throw new NotFoundException($"Parent category with id {request.dto.ParentCategoryId} not found");

            // find the root of the tree
            var rootCategoryId = parentCategory.RootCategoryId ?? parentCategory.Id;

            // Check if name exists in this tree (excluding current category)
            var nameExistsInTree = await categoryRepository.CategoryExistsByNameInTreeExcludingId(
                request.dto.Name, rootCategoryId, request.CategoryId);
            if (nameExistsInTree)
                throw new BadRequestException(
                    $"A category with name '{request.dto.Name}' already exists in this category tree");
        }


        // Check for circular reference
        var currentParentId = request.dto.ParentCategoryId;
        while (currentParentId != null)
        {
            if (currentParentId == request.CategoryId)
                throw new BadRequestException("Cannot set parent: circular reference detected");

            var parent = await categoryRepository.GetByIdAsync(currentParentId.Value);
            currentParentId = parent.ParentCategoryId;
        }

        
        var oldParentCategoryId = categoryToUpdate.ParentCategoryId;
        var oldRootCategoryId = categoryToUpdate.RootCategoryId;
        
        categoryToUpdate.Name = request.dto.Name;
        categoryToUpdate.Description = request.dto.Description;
        categoryToUpdate.ImageUrl = request.dto.ImageUrl;
        categoryToUpdate.ParentCategoryId = request.dto.ParentCategoryId;
        

        // set root category ID based on new parent
        if (request.dto.ParentCategoryId != null && parentCategory != null) // Use stored parent
        {
            categoryToUpdate.RootCategoryId = parentCategory.RootCategoryId ?? parentCategory.Id;
        }
        else
        {
            // It's now a root category, so RootCategoryId should be null
            categoryToUpdate.RootCategoryId = null;
        }
        
        List<Category> descendantsToUpdate;
        // Scenario 1: Subcategory → Root (was a child, now becomes root)
        if (oldParentCategoryId != null && request.dto.ParentCategoryId == null)
        {
            // Get all descendants following parent hierarchy
            descendantsToUpdate = await categoryRepository.GetDescendantsByCategoryId(categoryToUpdate.Id);
            
            // Update descendants to point to the new root
            foreach (var descendant in descendantsToUpdate)
            {
                descendant.RootCategoryId = categoryToUpdate.Id;
            }

            categoryToUpdate.RootCategoryId = null;
        }
        // Scenario 2: Root → Subcategory OR moving between subcategories in different trees
        else if (oldRootCategoryId != categoryToUpdate.RootCategoryId)
        {
            // Get all descendants by old root (they all have the same RootCategoryId)
            if (oldRootCategoryId != null)
            {
                descendantsToUpdate = await categoryRepository.GetCategoriesByRootIdAsync(oldRootCategoryId.Value);
            }
            else
            {
                // Was a root, get descendants by parent hierarchy
                descendantsToUpdate = await categoryRepository.GetDescendantsByCategoryId(categoryToUpdate.Id);
            }
            
            // Update descendants to point to the new root
            foreach (var descendant in descendantsToUpdate)
            {
                descendant.RootCategoryId = categoryToUpdate.RootCategoryId ?? categoryToUpdate.Id;
            }
        }

        try
        {
            categoryRepository.UpdateCategory(categoryToUpdate);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cache.RemoveAsync($"{CacheKeys.ALL_CATEGORIES}");
            await cache.RemoveAsync($"{CacheKeys.SUB_CATEGORIES}{categoryToUpdate.Id}");
            if (categoryToUpdate.ParentCategoryId == null)
                await cache.RemoveAsync($"{CacheKeys.MAIN_CATEGORIES}");
            if (categoryToUpdate.RootCategoryId == null)
                await cache.RemoveAsync($"{CacheKeys.CATEGORY_TREE}{categoryToUpdate.Id}");
            else
                await cache.RemoveAsync($"{CacheKeys.CATEGORY_TREE}{categoryToUpdate.RootCategoryId.Value}");

            return mapper.Map<CategoryResponse>(categoryToUpdate);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error while updating category: {e.Message}");
            throw;
        }
    }
}