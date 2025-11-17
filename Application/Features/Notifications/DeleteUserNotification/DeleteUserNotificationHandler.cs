using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Notifications.DeleteUserNotification;

public class DeleteUserNotificationHandler(
    INotificationRepository notificationRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteUserNotificationCommand>
{
    public async Task Handle(DeleteUserNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetNotification(request.NotificationId);
        if (notification == null)
        {
            throw new NotFoundException("Notification not found");
        }
    
        var notificationUser = notification.NotificationUsers
            .FirstOrDefault(nu => nu.UserId == request.UserId);

        if (notificationUser == null)
        {
            throw new BadRequestException("You did not receive this notification");
        }
    
        notificationRepository.DeleteNotificationUser(notificationUser);
    
        await unitOfWork.SaveChangesAsync();
        await cacheService.RemoveAsync($"{CacheKeys.NOTIFICATIONS}{request.UserId}");
    }
}