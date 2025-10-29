using FluentValidation;

namespace Application.Features.Categories.GetCategoryTree;

public class GetCategoryTreeValidator : AbstractValidator<GetCategoryTreeCommand>
{
    public GetCategoryTreeValidator()
    {
        RuleFor(x => x.RootCategoryId)
            .NotEmpty().WithMessage("Category ID cannot be empty")
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");
    }
}