using Application.Features.Listings.Shared.DeleteListing;
using Application.Features.Notifications.SendGlobalNotification;
using Domain.DTOs.Notifications;
using Infrastructure.Helpers.EmailTemplates;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminPanelController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost("global-notification")]
    public async Task<IActionResult> SendGlobalNotification(SendNotificationRequest request)
    {
        var command = new SendGlobalNotificationCommand
        {
            Dto = request,
            UserId = requestContextService.GetUserId()
        };
        
        await mediator.Send(command);
        return Ok();
    }
    
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> DeleteListing([FromRoute] int listingId)
    {
        var command = new DeleteListingCommand
        {
            ListingId = listingId,
            UserId = requestContextService.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }
}