using Domain.DTOs.User;

namespace Domain.DTOs.Messages;

public class ChatInfoResponse
{
    public int UnreadMessagesCount { get; set; }
    public LastMessageResponse LastMessage { get; set; }
    private List<ChatMemberResponse> Members { get; set; }
    public int ListingOwnerId { get; set; }
}