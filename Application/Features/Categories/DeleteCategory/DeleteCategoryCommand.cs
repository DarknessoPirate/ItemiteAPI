using MediatR;

namespace Application.Features.Categories.DeleteCategory;

public class DeleteCategoryCommand : IRequest
{
    public int CategoryId { get; set; }
    public bool DeleteFullTree { get; set; }
}