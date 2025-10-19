using FluentValidation;

namespace Application.Features.Messages.SendMessage;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.RecipientId)
            .NotEmpty().WithMessage("Recipient ID can't be null")
            .GreaterThan(0).WithMessage("Recipient ID must be valid");

        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender ID can't be null")
            .GreaterThan(0).WithMessage("Sender ID must be valid");

        RuleFor(x => x.Content)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Content))
            .WithMessage("Message length cannot exceed 1000 characters");

        // must have at least content OR photos, (can have only photos or only content) 
        RuleFor(x => x)
            .Must(cmd => !string.IsNullOrWhiteSpace(cmd.Content) || cmd.Photos.Count != 0)
            .WithMessage("Message must contain either text content or at least one photo");

        RuleFor(x => x.Photos)
            .Must(photos => photos.Count <= 6)
            .WithMessage("Maximum 6 photos allowed per message");

        RuleForEach(x => x.Photos)
            .Must(photo => photo != null && photo.Length > 0)
            .WithMessage("Invalid photo file")
            .Must(photo => photo.FileSizeInMB <= 10) // 10MB max per file
            .WithMessage("Photo file size cannot exceed 10MB");
    }
}