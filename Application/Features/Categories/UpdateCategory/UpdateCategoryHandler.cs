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

        // TODO: CHECK FOR NAME UNIQUENESS IN MAIN CATEGORIES AND IN TREE ALSO
        var nameExists = await categoryRepository.CategoryExistsByNameExcludingId(request.dto.Name, request.CategoryId);
        if (nameExists)
            throw new BadRequestException($"Category with name {request.dto.Name} already exists");

        if (request.dto.ParentCategoryId != null)
        {
            var parentExists = await categoryRepository.CategoryExistsById(request.dto.ParentCategoryId.Value);
            if (!parentExists)
                throw new NotFoundException($"Parent category with id {request.dto.ParentCategoryId} not found");
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

        categoryToUpdate.Name = request.dto.Name;
        categoryToUpdate.Description = request.dto.Description;
        categoryToUpdate.ImageUrl = request.dto.ImageUrl;
        categoryToUpdate.ParentCategoryId = request.dto.ParentCategoryId;
        // TODO : SET ROOT CATEGORY ID IF SETTING PARENT

        try
        {
            categoryRepository.UpdateCategory(categoryToUpdate);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await cache.RemoveAsync($"{CacheKeys.ALL_CATEGORIES}");
            await cache.RemoveAsync($"{CacheKeys.SUB_CATEGORIES}{categoryToUpdate.Id}");
            if (categoryToUpdate.ParentCategoryId == null)
                await cache.RemoveAsync($"{CacheKeys.MAIN_CATEGORIES}");
            if(categoryToUpdate.RootCategoryId == null)
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