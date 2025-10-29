using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Categories.DeleteCategory;

public class DeleteCategoryHandler(
    ICategoryRepository categoryRepository,
    ICacheService cache,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCategoryHandler> logger
) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryToDelete = await categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryToDelete == null)
            throw new NotFoundException($"Category with id: {request.CategoryId} does not exist");

        if (!request.DeleteFullTree && await categoryRepository.IsParentCategory(request.CategoryId))
            throw new BadRequestException(
                "You can't remove a category that is a parent. If you truly need to delete the whole tree, set the ForceDelete flag");

        try
        {
            categoryRepository.DeleteCategory(categoryToDelete);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete category with id: {CategoryId}", request.CategoryId);
            throw;
        }
        finally
        {
            await cache.RemoveAsync(CacheKeys.ALL_CATEGORIES);
            if (categoryToDelete.ParentCategoryId.HasValue && categoryToDelete.RootCategoryId.HasValue)
            {
                await cache.RemoveAsync($"{CacheKeys.SUB_CATEGORIES}{categoryToDelete.ParentCategoryId.Value}");
                await cache.RemoveAsync($"{CacheKeys.SUB_CATEGORIES}{request.CategoryId}");
                await cache.RemoveAsync($"{CacheKeys.CATEGORY_TREE}{categoryToDelete.RootCategoryId.Value}");
            }
            if (categoryToDelete.ParentCategoryId == null)
                await cache.RemoveAsync(CacheKeys.MAIN_CATEGORIES);
        }
    }
}