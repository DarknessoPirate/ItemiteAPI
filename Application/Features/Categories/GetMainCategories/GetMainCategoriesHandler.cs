using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services.Caching;
using MediatR;

namespace Application.Features.Categories.GetMainCategories;

public class GetMainCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetMainCategoriesCommand, List<CategoryResponse>>
{
    private const int CACHE_TIME_IN_MINUTES = 10;
    public async Task<List<CategoryResponse>> Handle(GetMainCategoriesCommand request, CancellationToken cancellationToken)
    {
       var cachedMainCategories = await cache.GetAsync<List<CategoryResponse>>("main_categories");
       if (cachedMainCategories != null)
       {
           return cachedMainCategories;
       }
       var mainCategories = await categoryRepository.GetMainCategories();
       var mappedMainCategories = mapper.Map<List<CategoryResponse>>(mainCategories);
       await cache.SetAsync("main_categories", mappedMainCategories, CACHE_TIME_IN_MINUTES);
       return mappedMainCategories;
    }
}