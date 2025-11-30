using Domain.DTOs.Notifications;
using MediatR;

namespace Application.Features.Notifications.SendGlobalNotification;

public class SendGlobalNotificationCommand : IRequest
{
    public int UserId { get; set; }
    public SendNotificationRequest Dto { get; set; }
}