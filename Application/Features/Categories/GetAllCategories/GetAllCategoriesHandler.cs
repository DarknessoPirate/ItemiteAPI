using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Categories.GetAllCategories;

public class GetAllCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper
    ) : IRequestHandler<GetAllCategoriesCommand, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetAllCategoriesCommand request, CancellationToken cancellationToken)
    {
       var categories = await categoryRepository.GetAllCategories(); 
       return mapper.Map<List<CategoryResponse>>(categories);
    }
}