using FluentValidation;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsValidator : AbstractValidator<GetPaginatedListingsQuery>
{
    public GetPaginatedListingsValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.Query.PriceFrom)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(q => q.Query.PriceTo ?? decimal.MaxValue)
            .When(q => q.Query.PriceFrom.HasValue);

        RuleFor(q => q.Query.PriceTo)
            .GreaterThanOrEqualTo(q => q.Query.PriceFrom ?? 0)
            .When(q => q.Query.PriceTo.HasValue);
    }
}