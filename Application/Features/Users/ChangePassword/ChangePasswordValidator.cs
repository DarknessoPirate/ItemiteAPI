using System.Data;
using FluentValidation;

namespace Application.Features.Users.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.UserId).NotNull().WithMessage("Incorrect user ID");

        RuleFor(x => x.dto.OldPassword)
            .NotEmpty().WithMessage("Old password is required");
        
        RuleFor(x => x.dto.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .Length(7, 50).WithMessage("Password field must be between 7 and 50 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

    }
}