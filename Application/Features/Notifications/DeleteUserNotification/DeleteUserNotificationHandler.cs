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
    
        await unitOfWork.BeginTransactionAsync();
        try
        {
            notificationRepository.DeleteNotificationUser(notificationUser);
            // notification user count will be 0 after SaveChangesAsync so I need to check if its currently 1
            if (await notificationRepository.GetNotificationUserCount(notification.Id) == 1)
            {
                // no more notification user relation entity -> notification entity can be deleted
                notificationRepository.DeleteNotification(notification);
            }

            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
        await cacheService.RemoveByPatternAsync($"{CacheKeys.NOTIFICATIONS}{request.UserId}*");
    }
}