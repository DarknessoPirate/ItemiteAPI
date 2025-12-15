using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Notifications.DeleteAllUserNotifications;

public class DeleteAllUserNotificationsHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteAllUserNotificationsCommand>
{
    public async Task Handle(DeleteAllUserNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userNotifications = await notificationRepository.GetUserNotifications(request.UserId);
    
        await unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var userNotification in userNotifications)
            {
                notificationRepository.DeleteNotificationUser(userNotification);
                // notification user count will be 0 after SaveChangesAsync so I need to check if its currently 1
                if (await notificationRepository.GetNotificationUserCount(userNotification.NotificationId) == 1)
                {
                    var notificationToDelete = 
                        await notificationRepository.GetNotification(userNotification.NotificationId) ??
                        throw new NotFoundException("Notification not found");
                    notificationRepository.DeleteNotification(notificationToDelete);
                }
            }

            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}