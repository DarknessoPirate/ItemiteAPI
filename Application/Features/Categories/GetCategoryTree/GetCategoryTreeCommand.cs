using Domain.DTOs.Category;
using MediatR;

namespace Application.Features.Categories.GetCategoryTree;

public class GetCategoryTreeCommand : IRequest<CategoryTreeResponse>
{
    public int RootCategoryId { get; set; }
}