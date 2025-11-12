using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Users.ChangeLocation;

public class ChangeLocationValidator : AbstractValidator<ChangeLocationCommand>
{
    public ChangeLocationValidator()
    {
        RuleFor(x => x.Dto.Location)
            .NotNull()
            .WithMessage("Location is required");
        
        RuleFor(x => x.Dto.Location.Longitude)
            .NotNull().WithMessage("Longitude is required")
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180");
        RuleFor(x => x.Dto.Location.Latitude)
            .NotNull().WithMessage("Latitude is required")
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90");
        RuleFor(x => x.Dto.Location.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2, 100)
            .WithMessage("Country must be between 2 and 100 characters");
        RuleFor(x => x.Dto.Location.City)
            .NotEmpty().WithMessage("City is required")
            .Length(2, 100)
            .WithMessage("City must be between 2 and 100 characters");
        RuleFor(x => x.Dto.Location.State)
            .NotEmpty().WithMessage("State is required")
            .Length(2, 100)
            .WithMessage("State must be between 2 and 100 characters");
        
    }
    
}