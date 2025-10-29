using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Categories.GetAllCategories;

public class GetAllCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetAllCategoriesCommand, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetAllCategoriesCommand request, CancellationToken cancellationToken)
    {
       var cachedCategories = await cache.GetAsync<List<CategoryResponse>>(CacheKeys.ALL_CATEGORIES);
       if (cachedCategories != null)
       {
           return cachedCategories;
       }
       var categories = await categoryRepository.GetAllCategories();
       var mappedCategories = mapper.Map<List<CategoryResponse>>(categories);
       await cache.SetAsync(CacheKeys.ALL_CATEGORIES, mappedCategories);
       return mappedCategories;
    }
}