using Application.Features.Notifications.GetUserNotifications;
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
}