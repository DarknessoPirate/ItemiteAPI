using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryResponse>
{
    public int CategoryId { get; set; }
    public UpdateCategoryRequest dto;
}