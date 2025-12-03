using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.Payments.DisputePurchase;

public class DisputePurchaseValidator : AbstractValidator<DisputePurchaseCommand>
{
    public DisputePurchaseValidator()
    {
        RuleFor(d => d.PaymentId)
            .GreaterThan(0).WithMessage("Invalid payment id");
        RuleFor(d => d.Reason)
            .IsInEnum().WithMessage("Invalid dispute reason");
        RuleFor(d => d.Description)
            .NotEmpty().WithMessage("Dispute description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        RuleFor(x => x.EvidencePhotos)
            .Must(photos => photos == null || photos.Count <= 6)
            .WithMessage("Maximum 6 evidence photos allowed");
        RuleForEach(x => x.EvidencePhotos)
            .Must(photo => photo.FileSizeInMB < 10) // 5MB
            .WithMessage("Each photo must be less than 10MB")
            .When(x => x.EvidencePhotos != null);
    }
}