using FluentValidation;

namespace Application.Features.Payments.GetUserSales;

public class GetUserSalesValidator : AbstractValidator<GetUserSalesQuery>
{
    public GetUserSalesValidator()
    {
        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");
    }
}