using AutoMapper;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = mapper.Map<Category>(command.CreateCategoryDto);
        var dto = command.CreateCategoryDto;
        
        var categoryExists = await categoryRepository.CategoryExistsByName(dto.Name);
        if (categoryExists)
            throw new BadRequestException("Category with that name already exists");
        
        if (dto.ParentCategoryId != null)
        {
            var parentCategory = await categoryRepository.GetByIdAsync(dto.ParentCategoryId!.Value);
            if(parentCategory == null)
                throw new NotFoundException("Parent category not found");
            
            // if parent has a root reference set the new category root to the same id, if not the parent is the root
            category.RootCategoryId = parentCategory.RootCategoryId ?? parentCategory.Id; 
        }


        await categoryRepository.CreateCategory(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        
        // remove cache after adding new entity for getting fresh data 
        if (category.ParentCategoryId != null)
            await cache.RemoveAsync($"sub_categories_{category.ParentCategoryId.Value}");
        else if (category.RootCategoryId != null)
            await cache.RemoveAsync($"category_tree_{category.RootCategoryId.Value}");
        else
            await cache.RemoveAsync("main_categories");
        
        await cache.RemoveAsync("all_categories");

        return category.Id;
    }            
}            