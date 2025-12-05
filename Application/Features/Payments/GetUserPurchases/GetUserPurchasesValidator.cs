using FluentValidation;

namespace Application.Features.Payments.GetUserPurchases;

public class GetUserPurchasesValidator : AbstractValidator<GetUserPurchasesQuery>
{
    public GetUserPurchasesValidator()
    {
        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");
    }
}