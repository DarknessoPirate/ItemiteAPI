using FluentValidation;

namespace Application.Features.Auth.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.loginDto.Email)
            .NotEmpty().WithMessage("Email field is required")
            .NotNull().WithMessage("Email field cannot be null")
            .Length(3, 100).WithMessage("Email field must be between 3 and 100 characters")
            .EmailAddress().WithMessage("Incorrect email format");

        RuleFor(x => x.loginDto.Password)
            .NotEmpty().WithMessage("Password field is required")
            .NotNull().WithMessage("Password field cannot be null");
    }
}