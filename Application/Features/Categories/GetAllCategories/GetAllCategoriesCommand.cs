using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.GetAllCategories;

public class GetAllCategoriesCommand : IRequest<List<CategoryResponse>>
{
    
}