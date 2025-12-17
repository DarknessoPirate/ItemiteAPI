using Domain.DTOs.User;
using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IUserRepository
{
    Task RemoveProfilePhotoAsync(int userId);
    Task RemoveBackgroundPhotoAsync(int userId);
    Task<User?> GetUserWithProfilePhotoAsync(int userId);
    Task<User?> GetUserWithBackgroundPhotoAsync(int userId);
    Task<User?> GetUserWithAllFieldsAsync(int userId);
    Task<List<User>> GetAllUsers();
    IQueryable<User> GetUsersQueryable();
    Task<Dictionary<int, ChatMemberInfo>> GetUsersInfoAsync(List<int> userIds);
}