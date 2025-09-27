using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    private readonly UserManager<User> _userManager;

    public RegisterValidator(UserManager<User> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.registerDto.Email)
            .NotEmpty().WithMessage("Email field is required")
            .NotNull().WithMessage("Email field cannot be null")
            .Length(3, 100).WithMessage("Email field must be between 3 and 100 characters")
            .EmailAddress().WithMessage("Incorrect email format")
            .MustAsync(EmailIsUnique).WithMessage("Email already in use");

        RuleFor(x => x.registerDto.UserName)
            .NotEmpty().WithMessage("Username field is required")
            .NotNull().WithMessage("Username field cannot be null")
            .Length(3, 20).WithMessage("Username field must be between 3 and 20 characters")
            .Matches("^[a-zA-Z0-9._@+-]+$").WithMessage("Username can only contain letters, numbers and characters .-_@+")
            .MustAsync(UsernameIsUnique).WithMessage("Username is already taken");

        RuleFor(x => x.registerDto.Password)
            .NotEmpty().WithMessage("Password field is required")
            .NotNull().WithMessage("Password field cannot be null")
            .Length(7, 50).WithMessage("Password field must be between 7 and 50 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        
        // TODO: PhoneNumber and Location string validation 
    }

    private async Task<bool> EmailIsUnique(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        return existingUser == null;
    }

    private async Task<bool> UsernameIsUnique(string username, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        return existingUser == null;
    }
}