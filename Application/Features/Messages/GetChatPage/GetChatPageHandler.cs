using AutoMapper;
using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Messages.GetChatPage;

/*
 * This will return paginated messages from the specific chat between 2 users about one listing, 
 * It is designed to work with another handler that gives you the appropriate data to fetch the chat page
 * like the GetListingChatsHandler (get all listing owner's chats for the listing) or GetUserChatsHandler (get all chats user is a part of)
 * IMPORTANT: THIS IS A PAGINATED RESPONSE
 */
public class GetChatPageHandler(
    UserManager<User> userManager,
    IMessageRepository messageRepository,
    IListingRepository<ListingBase> listingRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork
) : IRequestHandler<GetChatPageQuery, CursorPageResponse<MessageResponse>>
{
    public async Task<CursorPageResponse<MessageResponse>> Handle(GetChatPageQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");

        var listing = await listingRepository.GetListingByIdAsync(request.ListingId);
        if (listing == null)
            throw new BadRequestException("Listing not found");

        if (listing.OwnerId != request.UserId && listing.OwnerId != request.OtherUserId)
            throw new UnauthorizedException("You cannot access messages for this listing");

        var messages = await messageRepository.FindMessagesBetweenUsersAsync(
            request.UserId,
            request.OtherUserId,
            request.ListingId,
            request.Cursor,
            request.Limit + 1); // fetching one more message to check if the next cursor exists
        
        bool hasMore = messages.Count > request.Limit;
        
        
        // skip additional message which was fetched on 45 line
        var messagesToReturn = hasMore ? messages.Skip(1).ToList() : messages;
        
        var unreadMessages = messagesToReturn.Where(m => m.IsRead == false && m.SenderId != request.UserId).ToList();
        var readDate = DateTime.UtcNow;
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = readDate;
            messageRepository.Update(message);
        }

        await unitOfWork.SaveChangesAsync();
        
        var messagesResponse = mapper.Map<List<MessageResponse>>(messagesToReturn);
        
        
        string? nextCursor = null;
        if (hasMore)
        {
            var oldestMessage = messagesToReturn.First();
            nextCursor = Cursor.Encode(oldestMessage.DateSent, oldestMessage.Id);
        }

        return new CursorPageResponse<MessageResponse>(
           messagesResponse,
           nextCursor,
           messagesToReturn.Count,
           request.Limit
        );
    }
}