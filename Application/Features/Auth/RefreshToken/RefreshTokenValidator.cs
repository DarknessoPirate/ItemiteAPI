using FluentValidation;

namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.TokenPair.AccessToken)
            .NotEmpty().WithMessage("Access token field is required")
            .NotNull().WithMessage("Access token field cannot be null");
        RuleFor(x => x.TokenPair.RefreshToken)
            .NotEmpty().WithMessage("Refresh token field is required")
            .NotNull().WithMessage("Refresh token field cannot be null");
    }
}