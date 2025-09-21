using Domain.DTOs.Category;
using FluentValidation;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IValidator<CreateCategoryRequest> dtoValidator)
    {
        RuleFor(x => x.CreateCategoryDto)
            .SetValidator(dtoValidator);
    }
}