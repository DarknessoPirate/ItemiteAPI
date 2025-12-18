using Domain.DTOs.Category;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryCommand : IRequest<int>
{
    public CreateCategoryRequest CreateCategoryDto { get; set; }
    public FileWrapper? Image {get; set;}
}