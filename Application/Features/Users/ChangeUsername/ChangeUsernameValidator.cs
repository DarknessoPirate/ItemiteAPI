using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.ChangeUsername;

public class ChangeUsernameValidator : AbstractValidator<ChangeUsernameCommand>
{
    private readonly UserManager<User> _userManager;
    public ChangeUsernameValidator(UserManager<User> userManager)
    {
        _userManager = userManager;
        
        RuleFor(x => x.Dto.NewUsername)
            .NotEmpty().WithMessage("Username field is required")
            .NotNull().WithMessage("Username field cannot be null")
            .Length(3, 20).WithMessage("Username field must be between 3 and 20 characters")
            .Matches("^[a-zA-Z0-9._@+-]+$")
            .WithMessage("Username can only contain letters, numbers and characters .-_@+")
            .MustAsync(UsernameIsUnique).WithMessage("Username is already taken");
    }
    
    private async Task<bool> UsernameIsUnique(string username, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        return existingUser == null;
    }
}