using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryCommand : IRequest<int>
{
    public CreateCategoryRequest CreateCategoryDto { get; set; }
}