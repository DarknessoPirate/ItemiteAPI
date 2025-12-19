using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Exceptions;
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

    public async Task<(List<Message>, int)> FindLatestMessagesByListingIdAsync(int listingId, int pageNumber,
        int pageSize)
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

    public async Task<(List<Message>, int)> FindLatestMessagesForUserIdAsync(int userId, int pageNumber, int pageSize, Perspective perspective)
    {
        var allMessages = await dbContext.Messages
            .Include(m => m.Listing)
            .Where(m => (m.SenderId == userId || m.RecipientId == userId)
                        && (perspective == Perspective.Buyer 
                            ? m.Listing.OwnerId != userId 
                            : m.Listing.OwnerId == userId)
                        )
            .Select(m => new
            {
                m.Id,
                m.SenderId,
                m.RecipientId,
                m.ListingId,
                m.DateSent
            })
            .ToListAsync();

        // group them by users and listing id (2 users can have chats for multiple listings between each other)
        var latestMessageIds = allMessages
            .GroupBy(m => new
            {
                User1 = m.SenderId < m.RecipientId ? m.SenderId : m.RecipientId,
                User2 = m.SenderId < m.RecipientId ? m.RecipientId : m.SenderId,
                m.ListingId // Added to grouping key!
            })
            .Select(g => g.OrderByDescending(m => m.DateSent).First().Id)
            .ToList();

        // total chats count
        var totalCount = latestMessageIds.Count;

        var paginatedMessageIds = latestMessageIds
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // fetch full messages for the paginated ids
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


    public async Task<List<UnreadMessageCount>> GetUnreadMessageCountsForListingIdAsync(int listingId, int recipientId)
    {
        var unreadCounts = await dbContext.Messages
            .Where(m => m.ListingId == listingId
                        && m.RecipientId == recipientId
                        && !m.IsRead
            )
            .GroupBy(m => m.SenderId)
            .Select(g => new UnreadMessageCount
            {
                UserId = recipientId,
                OtherUserId = g.Key,
                ListingId = listingId,
                Count = g.Count()
            })
            .ToListAsync();

        return unreadCounts;
    }
    
    public async Task<int> GetUnreadCountAsync(int listingId, int senderId, int recipientId)
    {
        return await dbContext.Messages
            .Where(m => 
                m.ListingId == listingId &&
                m.SenderId == senderId &&
                m.RecipientId == recipientId &&
                !m.IsRead)
            .CountAsync();
    }

    public async Task<List<UnreadMessageCount>> GetUnreadMessageCountsForUserIdAsync(int userId, Perspective perspective)
    {
        var unreadCounts = await dbContext.Messages
            .Include(m => m.Listing)
            .Where(m => m.RecipientId == userId
                        && !m.IsRead
                        && (perspective == Perspective.Buyer 
                            ? m.Listing.OwnerId != userId 
                            : m.Listing.OwnerId == userId))
            .GroupBy(m => new { m.SenderId, m.ListingId })
            .Select(g => new UnreadMessageCount
            {
                UserId = userId,
                OtherUserId = g.Key.SenderId,
                ListingId = g.Key.ListingId,
                Count = g.Count()
            })
            .ToListAsync();

        return unreadCounts;
    }

    public async Task<int> GetMessageCountBetweenUsersAsync(int userId, int otherUserId, int listingId)
    {
        return await dbContext.Messages
            .Where(m => m.ListingId == listingId &&
                        ((m.RecipientId == userId && m.SenderId == otherUserId) ||
                         (m.RecipientId == otherUserId && m.SenderId == userId)))
            .CountAsync();
    }

    public async Task<List<Message>> FindMessagesBetweenUsersAsync(int userId, int otherUserId, int listingId,
        string? cursor, int limit)
    {
        var query = dbContext.Messages
            .Where(m => m.ListingId == listingId &&
                        ((m.RecipientId == userId && m.SenderId == otherUserId) ||
                         (m.RecipientId == otherUserId && m.SenderId == userId)));
    
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            var decodedCursor = Cursor.Decode(cursor);
            if (decodedCursor == null)
            {
                throw new BadRequestException("Invalid cursor");
            }
        
            query = query.Where(m => m.DateSent < decodedCursor.DateSent || 
                                     (m.DateSent == decodedCursor.DateSent && m.Id < decodedCursor.LastId));
        }
        
        var messageIds = await query
            .OrderByDescending(m => m.DateSent)
            .ThenByDescending(m => m.Id)
            .Take(limit)
            .Select(m => m.Id)
            .ToListAsync();
        
        var messages = await dbContext.Messages
            .Where(m => messageIds.Contains(m.Id))
            .Include(m => m.MessagePhotos)
            .ThenInclude(mp => mp.Photo)
            .OrderBy(m => m.DateSent)
            .ThenBy(m => m.Id)
            .ToListAsync();

        return messages;
    }
    
    public async Task<bool> HasUserMessagedAboutListingAsync(int senderId, int recipientId, int listingId)
    {
        return await dbContext.Messages
            .AnyAsync(m => m.SenderId == senderId
                           && m.RecipientId == recipientId
                           && m.ListingId == listingId);
    }

    public async Task<List<Photo>> FindPhotosByMessageIdAsync(int messageId)
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