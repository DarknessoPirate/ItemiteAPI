using FluentValidation;

namespace Application.Features.Auth.EmailConfirmation;

public class EmailConfirmationValidator : AbstractValidator<EmailConfirmationCommand>
{
    public EmailConfirmationValidator()
    {
        RuleFor(x => x.EmailConfirmationRequest.Token)
            .NotEmpty().WithMessage("Email token field is required")
            .NotNull().WithMessage("Email token field cannot be null");
        RuleFor(x => x.EmailConfirmationRequest.Email)
            .NotEmpty().WithMessage("Email token field is required")
            .NotNull().WithMessage("Email token field cannot be null")
            .EmailAddress().WithMessage("Incorrect email format");
    }    
}