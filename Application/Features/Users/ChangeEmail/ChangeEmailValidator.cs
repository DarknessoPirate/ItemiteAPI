using FluentValidation;

namespace Application.Features.Users.ChangeEmail;

public class ChangeEmailValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailValidator()
    {
        RuleFor(r => r.UserId).NotEmpty();
        
        RuleFor(r => r.changeEmailRequest.NewEmail)
            .NotEmpty().WithMessage("New email cannot be empty")
            .EmailAddress().WithMessage("Invalid email address format");
        
        RuleFor(r => r.changeEmailRequest.Password)
            .NotEmpty().WithMessage("Password cannot be empty");
    }
}