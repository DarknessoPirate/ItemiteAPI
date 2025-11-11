using FluentValidation;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsValidator : AbstractValidator<GetPaginatedListingsQuery>
{
    public GetPaginatedListingsValidator()
    {
        RuleFor(q => q.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.PriceFrom)
            .GreaterThan(0)
            .LessThanOrEqualTo(q => q.PriceTo ?? decimal.MaxValue)
            .When(q => q.PriceFrom.HasValue);

        RuleFor(q => q.PriceTo)
            .GreaterThanOrEqualTo(q => q.PriceFrom ?? 0)
            .When(q => q.PriceTo.HasValue);
    }
}