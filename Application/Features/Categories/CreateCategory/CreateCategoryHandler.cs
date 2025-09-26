using AutoMapper;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services.Caching;
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
        await categoryRepository.CreateCategory(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        
        // remove cache after adding new entity for getting fresh data 
        if (command.CreateCategoryDto.ParentCategoryId.HasValue)
        {
            
            await cache.RemoveAsync($"sub_categories_{command.CreateCategoryDto.ParentCategoryId.Value}");
            await cache.RemoveAsync("all_categories");
        }
        else
        {
            await cache.RemoveAsync("main_categories");
            await cache.RemoveAsync("all_categories");
        }

        return category.Id;
    }            
}            