using FluentValidation;

namespace Application.Features.Payments.ConfirmDelivery;

public class ConfirmDeliveryValidator : AbstractValidator<ConfirmDeliveryCommand>
{
    public ConfirmDeliveryValidator()
    {
        RuleFor(c => c.ListingId).GreaterThan(0).WithMessage("Invalid listing id");
    }
}