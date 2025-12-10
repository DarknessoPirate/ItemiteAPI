using FluentValidation;

namespace Application.Features.Reports.CreateReport;

public class CreateReportValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportValidator()
    {
        RuleFor(r => r.Images)
            .Must(images => images == null || images.Count <= 6)
            .WithMessage("You can't add more than 6 images to the report");
        RuleFor(r => r.UserId).NotNull().NotEmpty();
        RuleFor(r => r.ReportDto).NotNull().WithMessage("Report dto cannot be null")
            .NotEmpty().WithMessage("Report dto cannot be empty");
        RuleFor(r => r.ReportDto.Content).NotNull().WithMessage("Report content cannot be null")
            .NotEmpty().WithMessage("Report content cannot be empty")
            .MaximumLength(500).WithMessage("Report content length must be less than 500");
        RuleFor(r => r.ReportDto.ResourceType).NotNull().WithMessage("Report content cannot be null")
            .NotEmpty().WithMessage("Report content cannot be empty")
            .IsInEnum().WithMessage("Invalid resource type");

    }
}