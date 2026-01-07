using FluentValidation;

namespace Application.Features.Categories.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID cannot be empty")
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");
        
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2,50).WithMessage("Category name must be between 2 and 50 characters");
        
        RuleFor(x => x.Dto.PolishName)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(2,50).WithMessage("Category name must be between 2 and 50 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(100).WithMessage("Category description must have at most 100 characters");
    }
}