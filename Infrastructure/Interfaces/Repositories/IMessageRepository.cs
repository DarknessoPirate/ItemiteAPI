using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> FindByIdAsync(int messageId);
    Task<Message?> FindByIdWithPhotosAsync(int messageId);
    Task<(List<Message>, int)> FindLatestMessagesByListingIdAsync(int listingId, int pageNumber, int pageSize);
    Task<(List<Message>, int)> FindLatestMessagesForUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<List<UnreadMessageCount>> GetUnreadMessageCountsForListingIdAsync(int listingId, int recipientId);
    Task<int> GetUnreadCountAsync(int listingId, int senderId, int recipientId);
    Task<List<UnreadMessageCount>> GetUnreadMessageCountsForUserIdAsync(int userId);
    Task<int> GetMessageCountBetweenUsersAsync(int userId, int otherUserId, int listingId);
    Task<List<Message>> FindMessagesBetweenUsersAsync(int userId, int otherUserId, int listingId, string? cursor, int limit);
    Task<List<Photo>> FindPhotosByMessageIdAsync(int messageId);
    Task<bool> HasUserMessagedAboutListingAsync(int senderId, int recipientId, int listingId);
    void Update(Message message);
}