using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> FindByIdAsync(int messageId);
    Task<Message?> FindByIdWithPhotosAsync(int messageId);
    Task<List<Photo>> FindPhotosByMessageId(int messageId);
    void Update(Message message);
}