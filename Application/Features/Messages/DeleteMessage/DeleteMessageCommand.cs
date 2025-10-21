using MediatR;

namespace Application.Features.Messages.DeleteMessage;

public class DeleteMessageCommand : IRequest
{
    public int UserId { get; set; }
    public int MessageId { get; set; }
}