using Domain.DTOs.File;
using Domain.DTOs.Messages;
using MediatR;

namespace Application.Features.Messages.SendMessage;

public class SendMessageCommand : IRequest<SendMessageResult>
{
   public SendMessageRequest SendMessageDto { get; set; } 
   public int SenderId { get; set; }
   public List<FileWrapper> Photos { get; set; } = [];
}