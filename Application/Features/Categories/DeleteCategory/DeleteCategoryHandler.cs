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
    IUnitOfWork unitOfWork
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
        
        categoryRepository.DeleteCategory(categoryToDelete);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
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