using FluentValidation;

namespace Application.Features.Messages.GetListingChats;

public class GetListingChatsValidator : AbstractValidator<GetListingChatsCommand>
{

    public GetListingChatsValidator()
    {
        RuleFor(x => x.ListingId)
            .GreaterThan(0).WithMessage("Invalid listing id");
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user id");
    }
}