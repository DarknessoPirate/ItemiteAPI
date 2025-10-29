using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Categories.GetMainCategories;

public class GetMainCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetMainCategoriesCommand, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetMainCategoriesCommand request, CancellationToken cancellationToken)
    {
       var cachedMainCategories = await cache.GetAsync<List<CategoryResponse>>(CacheKeys.MAIN_CATEGORIES);
       if (cachedMainCategories != null)
       {
           return cachedMainCategories;
       }
       var mainCategories = await categoryRepository.GetMainCategories();
       var mappedMainCategories = mapper.Map<List<CategoryResponse>>(mainCategories);
       await cache.SetAsync(CacheKeys.MAIN_CATEGORIES, mappedMainCategories);
       return mappedMainCategories;
    }
}