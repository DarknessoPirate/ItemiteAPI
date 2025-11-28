using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Messages.GetChatPage;

public class GetChatPageQuery : IRequest<CursorPageResponse<MessageResponse>>
{
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public int OtherUserId { get; set; }
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
}