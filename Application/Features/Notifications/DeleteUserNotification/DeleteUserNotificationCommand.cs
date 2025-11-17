using MediatR;

namespace Application.Features.Notifications.DeleteUserNotification;

public class DeleteUserNotificationCommand : IRequest
{
    public int UserId { get; set; }
    public int NotificationId { get; set; }
}