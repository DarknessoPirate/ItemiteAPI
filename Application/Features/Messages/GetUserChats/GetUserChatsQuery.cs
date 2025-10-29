using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Messages.GetUserChats;

public class GetUserChatsQuery : IRequest<PageResponse<ChatInfoResponse>>
{
    public int UserId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}