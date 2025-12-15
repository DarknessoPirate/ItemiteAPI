using MediatR;

namespace Application.Features.Notifications.DeleteAllUserNotifications;

public class DeleteAllUserNotificationsCommand : IRequest
{
    public int UserId { get; set; }
}