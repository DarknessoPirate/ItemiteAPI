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

namespace Application.Features.Messages.GetUserChats;

/*
 * This handler is supposed to be used by a buyer , trying to find existing chats
 * for a product he wants to buy. This will return the same type as the GetListingChats
 * that the owner of a listing would use, but it is not confined to a specific listing.
 * The user will get all chats that he is a part of for all of the listings (excluding owned listings).
 * TLDR: use it to get chats for listings user wants to buy (excludes owned listing chats)
 * IMPORTANT: THIS IS A PAGINATED RESPONSE
 */
public class GetUserChatsHandler(
    UserManager<User> userManager,
    IMessageRepository messageRepository,
    IMapper mapper
) : IRequestHandler<GetUserChatsQuery, PageResponse<ChatInfoResponse>>
{
    public async Task<PageResponse<ChatInfoResponse>> Handle(GetUserChatsQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        var (latestMessages, totalChatsCount) =
            await messageRepository.FindLatestMessagesForUserIdAsync(request.UserId, request.PageNumber,
                request.PageSize, request.Perspective);

        var unreadCounts = await messageRepository.GetUnreadMessageCountsForUserIdAsync(request.UserId, request.Perspective);

        var chats = latestMessages.Select(message =>
        {
            // Determine who the other user is (conversation partner)
            var otherUser = message.SenderId == user.Id ? message.Recipient : message.Sender;

            var unreadCount = unreadCounts
                .FirstOrDefault(uc => uc.OtherUserId == otherUser.Id)
                ?.Count ?? 0;

            return new ChatInfoResponse
            {
                Listing = mapper.Map<ListingBasicInfo>(message.Listing),
                UnreadMessagesCount = unreadCount,
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
            totalChatsCount,
            request.PageSize,
            request.PageNumber
        );
    }
}