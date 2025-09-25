using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Services.Caching;
using MediatR;

namespace Application.Features.Categories.GetAllCategories;

public class GetAllCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetAllCategoriesCommand, List<CategoryResponse>>
{
    private const int CACHE_TIME_IN_MINUTES = 10;
    public async Task<List<CategoryResponse>> Handle(GetAllCategoriesCommand request, CancellationToken cancellationToken)
    {
       var cachedCategories = await cache.GetAsync<List<CategoryResponse>>("all_categories");
       if (cachedCategories != null)
       {
           return cachedCategories;
       }
       var categories = await categoryRepository.GetAllCategories();
       var mappedCategories = mapper.Map<List<CategoryResponse>>(categories);
       await cache.SetAsync("all_categories", mappedCategories, CACHE_TIME_IN_MINUTES);
       return mappedCategories;
    }
}