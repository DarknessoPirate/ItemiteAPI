using Domain.DTOs.Notifications;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Notifications.SendGlobalNotification;

public class SendGlobalNotificationHandler(
    INotificationService notificationService,
    IEmailService emailService,
    UserManager<User> userManager,
    IUserRepository userRepository
        ) : IRequestHandler<SendGlobalNotificationCommand>
{
    public async Task Handle(SendGlobalNotificationCommand request, CancellationToken cancellationToken)
    {
        var admin = await userManager.FindByIdAsync(request.UserId.ToString());
        if (admin == null)
        {
            throw new NotFoundException("User not found");
        }

        var notificationInfo = new NotificationInfo
        {
            Message = request.Dto.Message
        };

        var recipients = await userRepository.GetAllUsers();

        await notificationService.SendNotification(recipients.Select(u => u.Id).ToList(), request.UserId,
            notificationInfo);
        
        await emailService.SendGlobalNotificationAsync(recipients, request.Dto.EmailSubject, request.Dto.Title, request.Dto.Message);
    }
}