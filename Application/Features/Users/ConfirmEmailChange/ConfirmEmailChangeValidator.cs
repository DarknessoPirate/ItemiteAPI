using FluentValidation;

namespace Application.Features.Users.ConfirmEmailChange;

public class ConfirmEmailChangeValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Valid user ID is required");
        
        RuleFor(x => x.request.Token)
            .NotEmpty().WithMessage("Valid token is required");
        
        RuleFor(x => x.request.CurrentEmail)
            .NotEmpty().WithMessage("Current email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}