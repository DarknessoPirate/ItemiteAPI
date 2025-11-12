using FluentValidation;

namespace Application.Features.Users.ChangePhoneNumber;

public class ChangePhoneNumberValidator : AbstractValidator<ChangePhoneNumberCommand>
{
    public ChangePhoneNumberValidator()
    {
        RuleFor(x => x.Dto.PhoneNumber)
            .NotEmpty().WithMessage("Phone number field is required")
            .NotNull().WithMessage("Phone number field cannot be null")
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .WithMessage("Phone number can only contain digits, spaces, and characters: + - ( )")
            .Length(7, 20)
            .WithMessage("Phone number must be between 7 and 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber));
    }
}