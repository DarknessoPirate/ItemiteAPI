using Domain.DTOs.Notifications;
using MediatR;

namespace Application.Features.Notifications.SendNotification;

public class SendNotificationCommand : IRequest
{
    public SendNotificationRequest SendNotificationDto {get; set;}
    public int RecipientId {get; set;}
    public int UserId {get; set;}
}