using Application.Features.Notifications.DeleteUserNotification;
using Application.Features.Notifications.GetUserNotifications;
using Application.Features.Notifications.GetUserUnreadNotificationsCount;
using Domain.DTOs.Notifications;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NotificationInfo>>> GetNotifications()
    {
        var query = new GetUserNotificationsQuery
        {
            UserId = requestContextService.GetUserId()
        };
        
        var notifications = await mediator.Send(query);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCount()
    {
        var query = new GetUserUnreadNotificationsCountQuery
        {
            UserId = requestContextService.GetUserId()
        };
        var unreadNotificationsCount = await mediator.Send(query);
        return Ok(new {unreadNotificationsCount});
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteUserNotification(int notificationId)
    {
        var command = new DeleteUserNotificationCommand
        {
            NotificationId = notificationId,
            UserId = requestContextService.GetUserId()
        };
        
        await mediator.Send(command);
        return NoContent();
    }
}