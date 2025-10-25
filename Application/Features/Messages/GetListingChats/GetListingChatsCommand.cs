using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using MediatR;

namespace Application.Features.Messages.GetListingChats;

public class GetListingChatsCommand : IRequest<PageResponse<ChatInfoResponse>>
{
    public int UserId { get; set; }
    public int ListingId { get; set; }
}