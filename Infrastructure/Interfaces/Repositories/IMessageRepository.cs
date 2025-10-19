using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
}