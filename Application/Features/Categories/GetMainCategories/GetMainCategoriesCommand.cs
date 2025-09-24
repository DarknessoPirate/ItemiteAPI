using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.GetMainCategories;

public class GetMainCategoriesCommand : IRequest<List<CategoryResponse>>
{
    
}