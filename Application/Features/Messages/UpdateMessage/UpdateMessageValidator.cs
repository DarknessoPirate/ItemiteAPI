using FluentValidation;

namespace Application.Features.Messages.UpdateMessage;

public class UpdateMessageValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Invalid user ID");
        RuleFor(x => x.MessageId).GreaterThan(0).WithMessage("Invalid message ID");
        RuleFor(x => x.NewContent)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.NewContent))
            .WithMessage("Message length cannot exceed 1000 characters");
    }
}