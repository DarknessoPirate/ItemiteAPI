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

    public async Task<(List<Message>, int)> FindLatestMessagesByListingId(int listingId, int pageNumber, int pageSize)
    {
        // fetch all existing messages data for listing
        var allMessages = await dbContext.Messages
            .Where(m => m.ListingId == listingId)
            .Select(m => new 
            { 
                m.Id, 
                m.SenderId, 
                m.RecipientId, 
                m.DateSent 
            })
            .ToListAsync();

        // group and get the latest message for each unique conversation 
        var latestMessageIds = allMessages
            .GroupBy(m => m.SenderId < m.RecipientId 
                ? new { User1 = m.SenderId, User2 = m.RecipientId }
                : new { User1 = m.RecipientId, User2 = m.SenderId })
            .Select(g => g.OrderByDescending(m => m.DateSent).First().Id)
            .ToList();

        var totalCount = latestMessageIds.Count;

        // paginate 
        var paginatedMessageIds = latestMessageIds
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        // fetch full latest(only the unique lastest ones - 1 per conversation) messages with relevant data 
        var messages = await dbContext.Messages
            .Where(m => paginatedMessageIds.Contains(m.Id))
            .Include(m => m.Sender)
                .ThenInclude(s => s.ProfilePhoto)
            .Include(m => m.Recipient)
                .ThenInclude(r => r.ProfilePhoto)
            .Include(m => m.Listing)
                .ThenInclude(l => l.ListingPhotos)
                    .ThenInclude(lp => lp.Photo)
            .OrderByDescending(m => m.DateSent)
            .ToListAsync();

        return (messages, totalCount);
    }

    public async Task<Dictionary<int, int>> GetUnreadMessageCountsForListingId(int listingId, int recipientId)
    {
        /* This returns unread counts for each conversation related to this listing
         * example return: {5:3, 10:2, 11:0} (3 unread messages from userId 5, 2 unread messages from userId 10,...)
         */
        var unreadCounts = await dbContext.Messages
            .Where(m => m.ListingId == listingId 
                && m.RecipientId == recipientId 
                && !m.IsRead
                )
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count);
        
        return unreadCounts;
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