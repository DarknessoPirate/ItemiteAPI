using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services.Caching;
using MediatR;

namespace Application.Features.Categories.GetSubCategories;

public class GetSubCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetSubCategoriesCommand, List<CategoryResponse>>
{
    private const int CACHE_TIME_IN_MINUTES = 10;
    public async Task<List<CategoryResponse>> Handle(GetSubCategoriesCommand command, CancellationToken cancellationToken)
    {
        var cachedSubCategories = await cache.GetAsync<List<CategoryResponse>>($"sub_categories_{command.ParentCategoryId}");
        if (cachedSubCategories != null)
        {
            return cachedSubCategories;
        }
        var subCategories = await categoryRepository.GetSubCategories(command.ParentCategoryId);
        var mappedSubCategories = mapper.Map<List<CategoryResponse>>(subCategories);
        await cache.SetAsync($"sub_categories_{command.ParentCategoryId}", mappedSubCategories, CACHE_TIME_IN_MINUTES);
        return mapper.Map<List<CategoryResponse>>(subCategories);
    }
}