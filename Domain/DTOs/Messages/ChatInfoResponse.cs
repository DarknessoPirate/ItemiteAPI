using Domain.DTOs.Listing;
using Domain.DTOs.User;

namespace Domain.DTOs.Messages;

public class ChatInfoResponse
{
    public ListingBasicInfo Listing { get; set; }
    public int UnreadMessagesCount { get; set; }
    public LastMessageInfo LastMessage { get; set; }
    public List<ChatMemberInfo> Members { get; set; }
}