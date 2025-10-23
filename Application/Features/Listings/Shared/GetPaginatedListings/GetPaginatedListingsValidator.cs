using FluentValidation;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsValidator : AbstractValidator<GetPaginatedListingsQuery>
{
    public GetPaginatedListingsValidator()
    {
        RuleFor(q => q.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.PriceFrom).GreaterThan(0).LessThanOrEqualTo(q => q.PriceTo);
        RuleFor(q => q.PriceTo).GreaterThan(q => q.PriceFrom);
    }
}