using FluentValidation;

namespace Application.Features.Banners.AddBanner;

public class AddBannerValidator : AbstractValidator<AddBannerCommand>
{
    public AddBannerValidator()
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
            .Must(photo => photo.Length > 0)
            .WithMessage("Invalid photo file")
            .Must(photo => photo.FileSizeInMB <= 10)
            .WithMessage("Photo size can't exceed 10MB");
    }
}