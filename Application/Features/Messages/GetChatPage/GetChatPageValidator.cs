using System.Data;
using FluentValidation;

namespace Application.Features.Messages.GetChatPage;

public class GetChatPageValidator : AbstractValidator<GetChatPageQuery>
{
    public GetChatPageValidator()
    {
        RuleFor(x => x.ListingId)
            .GreaterThan(0).WithMessage("Invalid listing id");
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user id");
        RuleFor(x => x.OtherUserId)
            .NotEqual(x => x.UserId).WithMessage("You can't specify yourself as the other user")
            .GreaterThan(0).WithMessage("Invalid other user id");
        RuleFor(x => x.Limit)
            .GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Invalid limit size");

    }
}