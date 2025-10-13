using FluentValidation;

namespace Application.Features.ProductListings.GetPaginatedProductListings;

public class GetPaginatedProductListingsValidator : AbstractValidator<GetPaginatedProductListingsQuery>
{
    public GetPaginatedProductListingsValidator()
    {
        RuleFor(q => q.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.PriceFrom).GreaterThan(0).LessThanOrEqualTo(q => q.PriceTo);
        RuleFor(q => q.PriceTo).GreaterThan(q => q.PriceFrom);
    }
}