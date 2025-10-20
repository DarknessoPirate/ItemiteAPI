using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MessageRepository(ItemiteDbContext dbContext) : IMessageRepository
{
    public async Task AddAsync(Message message)
    {
        await dbContext.Messages.AddAsync(message);
    }

    public async Task<Message?> FindByIdAsync(int messageId)
    {
        return await dbContext.Messages.FindAsync(messageId);
    }

    public async Task<Message?> FindByIdWithPhotosAsync(int messageId)
    {
        var message = await dbContext.Messages
            .Include(m => m.MessagePhotos)
            .ThenInclude(mp => mp.Photo)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        return message;
    }

    public async Task<List<Photo>> FindPhotosByMessageId(int messageId)
    {
        var photos = await dbContext.MessagePhotos
            .Where(mp => mp.MessageId == messageId)
            .Select(mp => mp.Photo)
            .ToListAsync();

        return photos;
    }

    public void Update(Message message)
    {
        dbContext.Update(message);
    }
}