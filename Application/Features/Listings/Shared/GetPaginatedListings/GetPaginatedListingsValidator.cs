using FluentValidation;

namespace Application.Features.Listings.Shared.GetPaginatedListings;

public class GetPaginatedListingsValidator : AbstractValidator<GetPaginatedListingsQuery>
{
    public GetPaginatedListingsValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.Query.PriceFrom).GreaterThan(0).LessThanOrEqualTo(q => q.Query.PriceTo);
        RuleFor(q => q.Query.PriceTo).GreaterThan(q => q.Query.PriceFrom);
    }
}