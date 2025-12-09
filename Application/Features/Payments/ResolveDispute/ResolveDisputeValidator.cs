using FluentValidation;

namespace Application.Features.Payments.ResolveDispute;

public class ResolveDisputeValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeValidator()
    {
        RuleFor(x => x.DisputeId)
            .GreaterThan(0).WithMessage("Invalid dispute ID");

        RuleFor(x => x.Resolution)
            .IsInEnum().WithMessage("Invalid resolution type");

        RuleFor(x => x.PartialRefundAmount)
            .NotNull().WithMessage("Partial refund amount is required for partial refunds")
            .GreaterThan(0).WithMessage("Refund amount must be greater than 0")
            .When(x => x.Resolution == Domain.Enums.DisputeResolution.PartialRefund);

        RuleFor(x => x.ReviewerNotes)
            .MaximumLength(500).WithMessage("Admin notes cannot exceed 500 characters");
    }
}