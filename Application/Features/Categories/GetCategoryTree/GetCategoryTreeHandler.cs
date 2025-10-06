using AutoMapper;
using Domain.DTOs.Category;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Categories.GetCategoryTree;

public class GetCategoryTreeHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper
) : IRequestHandler<GetCategoryTreeCommand, CategoryTreeResponse>
{
    public async Task<CategoryTreeResponse> Handle(GetCategoryTreeCommand request, CancellationToken cancellationToken)
    {
        var rootCategory = await categoryRepository.GetByIdAsync(request.RootCategoryId);

        if (rootCategory == null)
            throw new NotFoundException("Root category not found, there is no tree to return");

        if (rootCategory.ParentCategoryId != null)
            throw new BadRequestException("Category is not a root category");

        var subCategories = await categoryRepository.GetCategoriesByRootIdAsync(request.RootCategoryId);

        // dict for faster lookups
        var categoryDictionary = new Dictionary<int, CategoryTreeResponse>();
        // mapping the root category first to create the tree
        var treeRoot = mapper.Map<CategoryTreeResponse>(rootCategory);
        categoryDictionary[rootCategory.Id] = treeRoot;

        //map all subcategories and place them in the dict
        foreach (var subCategory in subCategories)
        {
            var treeElement = mapper.Map<CategoryTreeResponse>(subCategory);
            categoryDictionary[subCategory.Id] = treeElement;
        }

        foreach (var subCategory in subCategories)
        {
            if (subCategory.ParentCategoryId != null &&
                categoryDictionary.ContainsKey(subCategory.ParentCategoryId.Value))
            {
                var parent = categoryDictionary[subCategory.ParentCategoryId.Value];
                parent.SubCategories.Add(categoryDictionary[subCategory.Id]);
            }
        }

        return treeRoot;
    }
}