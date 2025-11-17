using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Notifications.GetUserUnreadNotificationsCount;

public class GetUserUnreadNotificationsCountHandler(
    INotificationRepository notificationRepository
    ) : IRequestHandler<GetUserUnreadNotificationsCountQuery, int>
{
    public async Task<int> Handle(GetUserUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
    {
        var unreadNotificationsCount = await notificationRepository.GetUserUnreadNotificationsCount(request.UserId);
        return unreadNotificationsCount;
    }
}