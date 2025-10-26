using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> FindByIdAsync(int messageId);
    Task<Message?> FindByIdWithPhotosAsync(int messageId);
    Task<(List<Message>, int)> FindLatestMessagesByListingId(int listingId, int pageNumber, int pageSize);
    Task<Dictionary<int, int>> GetUnreadMessageCountsForListingId(int listingId, int recipientId);
    Task<List<Photo>> FindPhotosByMessageId(int messageId);
    void Update(Message message);
}