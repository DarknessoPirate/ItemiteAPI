using FluentValidation;

namespace Application.Features.Payments.GetPaymentsByStatus;

public class GetPaymentsByStatusValidator : AbstractValidator<GetPaymentsByStatusQuery>
{
    public GetPaymentsByStatusValidator()
    {
        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");
        RuleFor(q => q.PaymentStatus)
            .NotNull().WithMessage("Payment status is required")
            .IsInEnum().WithMessage("Provide a valid PaymentStatus enum value");
    }
}