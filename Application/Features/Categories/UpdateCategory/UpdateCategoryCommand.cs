using Domain.DTOs.Category;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Categories.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryResponse>
{
    public int CategoryId { get; set; }
    public UpdateCategoryRequest Dto;
}