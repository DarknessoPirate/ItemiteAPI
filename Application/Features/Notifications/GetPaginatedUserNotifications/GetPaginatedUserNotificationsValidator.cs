using FluentValidation;

namespace Application.Features.Notifications.GetPaginatedUserNotifications;

public class GetPaginatedUserNotificationsValidator : AbstractValidator<GetPaginatedUserNotificationsQuery>
{
    public GetPaginatedUserNotificationsValidator()
    {
        RuleFor(q => q.Query.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}