using MediatR;

namespace Application.Features.Notifications.GetUserUnreadNotificationsCount;

public class GetUserUnreadNotificationsCountQuery : IRequest<int>
{
    public int UserId { get; set; }
}