using FluentValidation;

namespace Application.Features.Auth.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.resetPasswordRequest.Email)
            .NotEmpty().WithMessage("Email field is required")
            .NotNull().WithMessage("Email field cannot be null")
            .Length(3, 100).WithMessage("Email field must be between 3 and 100 characters")
            .EmailAddress().WithMessage("Incorrect email format");
        
        RuleFor(x => x.resetPasswordRequest.Password)
            .NotEmpty().WithMessage("Password field is required")
            .NotNull().WithMessage("Password field cannot be null")
            .Length(7, 50).WithMessage("Password field must be between 7 and 50 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}