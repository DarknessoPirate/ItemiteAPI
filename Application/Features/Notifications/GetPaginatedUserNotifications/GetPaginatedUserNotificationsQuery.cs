using Domain.DTOs.Notifications;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Notifications.GetPaginatedUserNotifications;

public class GetPaginatedUserNotificationsQuery : IRequest<PageResponse<NotificationInfo>>
{
    public PaginateNotificationsQuery Query { get; set; }
    public int UserId { get; set; }
}