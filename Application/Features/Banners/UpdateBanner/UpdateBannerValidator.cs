using FluentValidation;

namespace Application.Features.Banners.UpdateBanner;

public class UpdateBannerValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerValidator()
    {
        RuleFor(c => c.Dto.Name)
            .NotEmpty()
            .WithMessage("Provide a name for the banner")
            .MaximumLength(100)
            .WithMessage("Banner name cannot exceed 100 characters");

        RuleFor(c => c.Dto.Position)
            .IsInEnum()
            .WithMessage("Provide a correct enum value for banner position");

        RuleFor(c => c.BannerPhoto)
            .Must(photo => photo == null || photo.Length > 0)
            .WithMessage("Invalid photo file")
            .Must(photo => photo == null || photo.FileSizeInMB <= 10)
            .WithMessage("Photo size can't exceed 10MB");

    }
}