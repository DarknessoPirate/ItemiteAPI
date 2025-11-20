using FluentValidation;

namespace Application.Features.Listings.Shared.GetPaginatedFollowedListings;

public class GetPaginatedFollowListingsValidator : AbstractValidator<GetPaginatedFollowedListingsQuery>
{
    public GetPaginatedFollowListingsValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}