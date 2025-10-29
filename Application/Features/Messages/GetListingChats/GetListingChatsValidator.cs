using FluentValidation;

namespace Application.Features.Messages.GetListingChats;

public class GetListingChatsValidator : AbstractValidator<GetListingChatsQuery>
{

    public GetListingChatsValidator()
    {
        RuleFor(x => x.ListingId)
            .GreaterThan(0).WithMessage("Invalid listing id");
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user id");
        RuleFor(x => x.PageNumber)
            .GreaterThan(-1).WithMessage("Invalid page number");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Invalid page size");
    }
}