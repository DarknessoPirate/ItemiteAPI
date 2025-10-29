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

/*
 * Returns list of basic information for existing chats for the specific listing
 * (listing info, unread message count, last message simple data, chat members data)
 * this is supposed to be used by the listing owner to get an overview of any existing/unread conversations from the listing.
 * TLDR: seller uses this to get existing chats for the listing
 * IMPORTANT: RETURNS PAGINATED RESPONSE
 */
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
        if (listing == null)
            throw new BadRequestException("Listing not found");

        if (listing.Owner.Id != user.Id)
            throw new UnauthorizedException("You can only get chats for your own listings");

        var (latestMessages, totalCount) =
            await messageRepository.FindLatestMessagesByListingIdAsync(listing.Id, request.PageNumber,
                request.PageSize);


        var unreadCounts = await messageRepository.GetUnreadMessageCountsForListingIdAsync(request.ListingId, user.Id);

        var chats = latestMessages.Select(message =>
        {
            // Determine who the other user is (conversation partner)
            var otherUser = message.SenderId == user.Id ? message.Recipient : message.Sender;

            // Get unread count for this conversation
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
            totalCount,
            request.PageSize,
            request.PageNumber
        );
    }
}