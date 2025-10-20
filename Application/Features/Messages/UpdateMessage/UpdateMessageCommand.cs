using Domain.DTOs.Messages;
using MediatR;

namespace Application.Features.Messages.UpdateMessage;

public class UpdateMessageCommand : IRequest<MessageResponse>
{
    public int UserId { get; set; }
    public int MessageId { get; set; }
    public string? NewContent { get; set; }
    
}