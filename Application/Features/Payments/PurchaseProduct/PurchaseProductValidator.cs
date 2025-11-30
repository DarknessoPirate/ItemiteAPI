using FluentValidation;

namespace Application.Features.Payments.PurchaseProduct;

public class PurchaseProductValidator : AbstractValidator<PurchaseProductCommand>
{
    public PurchaseProductValidator()
    {
        RuleFor(x => x.ProductListingId)
            .NotEmpty().WithMessage("Product ID is required")
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("Payment method is required")
            .MinimumLength(3).WithMessage("Invalid payment method");
    }
}