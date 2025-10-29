using Domain.Entities;
using Domain.ValueObjects;
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
            .Matches("^[a-zA-Z0-9._@+-]+$")
            .WithMessage("Username can only contain letters, numbers and characters .-_@+")
            .MustAsync(UsernameIsUnique).WithMessage("Username is already taken");

        RuleFor(x => x.registerDto.Password)
            .NotEmpty().WithMessage("Password field is required")
            .NotNull().WithMessage("Password field cannot be null")
            .Length(7, 50).WithMessage("Password field must be between 7 and 50 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        
        RuleFor(x => x.registerDto.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .WithMessage("Phone number can only contain digits, spaces, and characters: + - ( )")
            .Length(7, 20)
            .WithMessage("Phone number must be between 7 and 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.registerDto.PhoneNumber));
        
        RuleFor(x => x.registerDto.Location)
            .Must(LocationIsCompleteOrNull)
            .WithMessage(
                "If location is provided, all fields (Longitude, Latitude, Country, City, PostalCode) must be filled")
            .When(x => x.registerDto.Location != null);

        RuleFor(x => x.registerDto.Location.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180")
            .When(x => x.registerDto.Location != null && HasAnyLocationField(x.registerDto.Location));

        RuleFor(x => x.registerDto.Location.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90")
            .When(x => x.registerDto.Location != null && HasAnyLocationField(x.registerDto.Location));

        RuleFor(x => x.registerDto.Location.Country)
            .NotEmpty().WithMessage("Country is required when location is provided")
            .Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
            .When(x => x.registerDto.Location != null && HasAnyLocationField(x.registerDto.Location));

        RuleFor(x => x.registerDto.Location.City)
            .NotEmpty().WithMessage("City is required when location is provided")
            .Length(2, 100).WithMessage("City must be between 2 and 100 characters")
            .When(x => x.registerDto.Location != null && HasAnyLocationField(x.registerDto.Location));

        RuleFor(x => x.registerDto.Location.State)
            .NotEmpty().WithMessage("State is required when location is provided")
            .Length(2, 100).WithMessage("State must be between 2 and 100 characters")
            .When(x => x.registerDto.Location != null && HasAnyLocationField(x.registerDto.Location));
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
    
    private bool HasAnyLocationField(Location location)
    {
        return location.Longitude.HasValue 
               || location.Latitude.HasValue 
               || !string.IsNullOrWhiteSpace(location.Country) 
               || !string.IsNullOrWhiteSpace(location.City) 
               || !string.IsNullOrWhiteSpace(location.State);
    }

    private bool LocationIsCompleteOrNull(Location? location)
    {
        if (location == null) return true;
    
        if (!HasAnyLocationField(location)) return true;
        
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.State);
    }
}