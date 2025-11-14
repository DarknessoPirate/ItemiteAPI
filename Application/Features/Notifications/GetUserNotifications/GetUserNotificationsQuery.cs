using Domain.DTOs.Notifications;
using MediatR;

namespace Application.Features.Notifications.GetUserNotifications;

public class GetUserNotificationsQuery : IRequest<List<NotificationInfo>>
{
    public int UserId { get; set; }
}