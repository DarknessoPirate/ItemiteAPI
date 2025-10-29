using FluentValidation;

namespace Application.Features.Messages.DeleteMessage;

public class DeleteMessageValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID");
        RuleFor(x => x.MessageId)
            .GreaterThan(0).WithMessage("Invalid message ID");
    }
}