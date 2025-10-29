using FluentValidation;
using Infrastructure.Interfaces.Repositories;

namespace Application.Features.Categories.GetSubCategories;

public class GetSubCategoriesValidator : AbstractValidator<GetSubCategoriesCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetSubCategoriesValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.ParentCategoryId).NotNull().WithMessage("Parent category ID cannot be null.")
            .NotEmpty().WithMessage("Parent category ID cannot be empty.")
            .GreaterThan(0).WithMessage("Parent category ID must be greater than zero.")
            .MustAsync(ParentCategoryExists)
            .WithMessage("Parent category does not exist");
    }

    private async Task<bool> ParentCategoryExists(int parentId, CancellationToken cancellationToken)
    {
        return await _categoryRepository.CategoryExistsById(parentId);
    }
}