using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Categories.GetSubCategories;

public class GetSubCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetSubCategoriesCommand, List<CategoryResponse>>
{

    public async Task<List<CategoryResponse>> Handle(GetSubCategoriesCommand command, CancellationToken cancellationToken)
    {
        var cachedSubCategories = await cache.GetAsync<List<CategoryResponse>>($"{CacheKeys.SUB_CATEGORIES}{command.ParentCategoryId}");
        if (cachedSubCategories != null)
        {
            return cachedSubCategories;
        }
        var subCategories = await categoryRepository.GetSubCategories(command.ParentCategoryId);
        var mappedSubCategories = mapper.Map<List<CategoryResponse>>(subCategories);
        await cache.SetAsync($"{CacheKeys.SUB_CATEGORIES}{command.ParentCategoryId}", mappedSubCategories);
        return mapper.Map<List<CategoryResponse>>(subCategories);
    }
}