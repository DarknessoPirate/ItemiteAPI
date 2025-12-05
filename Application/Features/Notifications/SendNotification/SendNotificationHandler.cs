using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Notifications.SendNotification;

public class SendNotificationHandler(
    UserManager<User> userManager,
    INotificationService notificationService,
    IEmailService emailService
    ) : IRequestHandler<SendNotificationCommand>
{
    public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var admin = await userManager.FindByIdAsync(request.UserId.ToString());
        if (admin == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var recipient = await userManager.FindByIdAsync(request.RecipientId.ToString());
        if (recipient == null)
        {
            throw new NotFoundException("Recipient not found");
        }

        var notificationInfo = new NotificationInfo
        {
            Message = request.SendNotificationDto.Message
        };

        await notificationService.SendNotification([recipient.Id], request.UserId,
            notificationInfo);
        
        await emailService.SendNotificationAsync(recipient, request.SendNotificationDto.EmailSubject, request.SendNotificationDto.Title, request.SendNotificationDto.Message);
    }
}