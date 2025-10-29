using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Messages.GetChatPage;

public class GetChatPageQuery : IRequest<PageResponse<MessageResponse>>
{
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public int OtherUserId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}