using FluentValidation;

namespace Application.Features.Users.GetPaginatedUsers;

public class GetPaginatedUsersValidator : AbstractValidator<GetPaginatedUsersQuery>
{
    public GetPaginatedUsersValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Page size must be between 0 and 100");
        RuleFor(q => q.Query.Search).NotNull().WithMessage("Search cannot be null");
    }
}