using FluentValidation;

namespace Application.Features.Messages.GetUserChats;

public class GetUserChatsValidator : AbstractValidator<GetUserChatsQuery>
{
    public GetUserChatsValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user id");
        RuleFor(x => x.PageNumber)
            .GreaterThan(-1).WithMessage("Invalid page number");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Invalid page size");
        RuleFor(x => x.Perspective)
            .IsInEnum().WithMessage("Invalid perspective");
    }
}