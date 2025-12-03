using FluentValidation;

namespace Application.Features.Reports.GetPaginatedReports;

public class GetPaginatedReportsValidator : AbstractValidator<GetPaginatedReportsQuery>
{
    public GetPaginatedReportsValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Page size must be between 0 and 100");
        RuleFor(q => q.Query.ResourceType).IsInEnum().WithMessage("Invalid resource type");
    }
}