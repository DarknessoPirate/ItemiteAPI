using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Categories.GetMainCategories;

public class GetMainCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper
    ) : IRequestHandler<GetMainCategoriesCommand, List<CategoryResponse>> 
{
    public async Task<List<CategoryResponse>> Handle(GetMainCategoriesCommand request, CancellationToken cancellationToken)
    {
       var mainCategories = await categoryRepository.GetMainCategories();
       return mapper.Map<List<CategoryResponse>>(mainCategories);
    }
}