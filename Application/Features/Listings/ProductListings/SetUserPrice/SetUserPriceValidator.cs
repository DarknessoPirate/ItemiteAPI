using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Listings.ProductListings.SetUserPrice;

public class SetUserPriceValidator : AbstractValidator<SetUserPriceCommand>
{
    private readonly UserManager<User> _userManager;
    public SetUserPriceValidator(UserManager<User> userManager)
    {
        _userManager = userManager;
        
        RuleFor(x => x.Dto).NotNull().WithMessage("Set user price reuqest is null");
        RuleFor(x => x.Dto.Price)
            .NotEmpty().WithMessage("Price is empty")
            .NotNull().WithMessage("Price is null")
            .GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is empty")
            .NotNull().WithMessage("User id is null")
            .MustAsync(UserExists).WithMessage("User not found");
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("Listing id is empty")
            .NotNull().WithMessage("Listing id is null");
    }

    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) != null;
    }
}