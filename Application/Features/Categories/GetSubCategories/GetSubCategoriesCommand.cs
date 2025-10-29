using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.GetSubCategories;

public class GetSubCategoriesCommand : IRequest<List<CategoryResponse>>
{
    public int ParentCategoryId;
}