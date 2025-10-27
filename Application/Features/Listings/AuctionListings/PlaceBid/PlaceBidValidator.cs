using FluentValidation;

namespace Application.Features.Listings.AuctionListings.PlaceBid;

public class PlaceBidValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidValidator()
    {
        RuleFor(b => b.BidDto).NotNull().WithMessage("Bid dto is empty");
        RuleFor(b => b.AuctionId)
            .NotNull().WithMessage("Auction id is empty")
            .GreaterThan(0).WithMessage("Auction id must be greater than 0");
        RuleFor(b => b.BidDto.Price)
            .NotNull().WithMessage("Price is empty")
            .GreaterThan(0).WithMessage("Price must be greater than 0");
    }    
}