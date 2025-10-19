using Application.Features.Messages.SendMessage;
using Domain.DTOs.File;
using Domain.DTOs.Messages;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MessageController(IMediator mediator ,IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SendMessageResult>> SendMessage(
        [FromForm] SendMessageRequest request,
        [FromForm] IFormFileCollection photos)
    {
        var command = new SendMessageCommand
        {
            Content = request.Content,
            RecipientId = request.RecipientId,
            SenderId = requestContextService.GetUserId(),
            Photos = photos.Select(p => new FileWrapper(
                p.FileName,
                p.Length,
                p.ContentType,
                p.OpenReadStream()
            )).ToList()
        };

        var result = await mediator.Send(command);

        return Ok(result);
    }
}