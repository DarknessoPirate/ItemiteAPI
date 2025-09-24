using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Categories.GetSubCategories;

public class GetSubCategoriesHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper
    ) : IRequestHandler<GetSubCategoriesCommand, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetSubCategoriesCommand command, CancellationToken cancellationToken)
    {
        var subCategories = await categoryRepository.GetSubCategories(command.ParentCategoryId);
        return mapper.Map<List<CategoryResponse>>(subCategories);
    }
}