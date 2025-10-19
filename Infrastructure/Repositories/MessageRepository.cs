using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Repositories;

public class MessageRepository(ItemiteDbContext dbContext) : IMessageRepository
{
    public async Task AddAsync(Message message)
    {
        await dbContext.Messages.AddAsync(message);
    }
    
}