using Application.Features.Messages.DeleteMessage;
using Application.Features.Messages.GetListingChats;
using Application.Features.Messages.SendMessage;
using Application.Features.Messages.UpdateMessage;
using Domain.DTOs.File;
using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
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
            SendMessageDto = request,
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

    [HttpPut("{messageId:int}")]
    public async Task<ActionResult<MessageResponse>> UpdateMessage(int messageId, string? newContent)
    {
        var command = new UpdateMessageCommand
        {
            UserId = requestContextService.GetUserId(),
            MessageId = messageId,
            NewContent = newContent
        };

        var result = await mediator.Send(command);

        return Ok(result);
    }

    [HttpDelete("{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var command = new DeleteMessageCommand
        {
            UserId = requestContextService.GetUserId(),
            MessageId = messageId
        };

        await mediator.Send(command);

        return NoContent();
    }

    [HttpGet("{listingId:int}/chats")]
    public async Task<ActionResult<PageResponse<ChatInfoResponse>>> GetListingChats([FromRoute] int listingId ,[FromQuery] int pageNumber,
        [FromQuery] int pageSize)
    {
        var query = new GetListingChatsQuery
        {
            UserId = requestContextService.GetUserId(),
            ListingId = listingId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await mediator.Send(query);

        return result;
    }
    
}