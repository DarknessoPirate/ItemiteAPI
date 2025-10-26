using AutoMapper;
using Domain.DTOs.Listing;
using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using Domain.DTOs.User;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Messages.GetListingChats;

public class GetListingChatsHandler(
    UserManager<User> userManager,
    IMessageRepository messageRepository,
    IListingRepository<ListingBase> listingRepository,
    IMapper mapper
) : IRequestHandler<GetListingChatsQuery, PageResponse<ChatInfoResponse>>
{
    public async Task<PageResponse<ChatInfoResponse>> Handle(GetListingChatsQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        var listing = await listingRepository.GetListingByIdAsync(request.ListingId);
        // null check is implemented in repository
        if (listing.Owner.Id != user.Id)
            throw new UnauthorizedException("You can only get chats for your own listings");

        var (latestMessages, totalCount) =
            await messageRepository.FindLatestMessagesByListingId(listing.Id, request.PageNumber, request.PageSize);


        var unreadCounts = await messageRepository.GetUnreadMessageCountsForListingId(request.ListingId, user.Id);

        var chats = latestMessages.Select(message =>
        {
            // Determine who the other user is (conversation partner)
            var otherUser = message.SenderId == user.Id ? message.Recipient : message.Sender;

            // Get unread count for this conversation
            var unreadCount = unreadCounts.GetValueOrDefault(otherUser.Id, 0);

            return new ChatInfoResponse
            {
                Listing = mapper.Map<ListingBasicInfo>(message.Listing),
                UnreadMessagesCounts = unreadCount,
                LastMessage = mapper.Map<LastMessageInfo>(message),
                Members = new List<ChatMemberInfo>
                {
                    mapper.Map<ChatMemberInfo>(user),
                    mapper.Map<ChatMemberInfo>(otherUser)
                }
            };
        }).ToList();

        return new PageResponse<ChatInfoResponse>(
            chats,
            totalCount,
            request.PageSize,
            request.PageNumber
        );
    }
}