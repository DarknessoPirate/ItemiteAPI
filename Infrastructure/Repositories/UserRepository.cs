using Domain.DTOs.User;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(ItemiteDbContext context) : IUserRepository
{
    public async Task RemoveProfilePhotoAsync(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user != null)
            user.ProfilePhotoId = null;
    }

    public async Task RemoveBackgroundPhotoAsync(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user != null)
            user.BackgroundPhotoId = null;
    }

    public async Task<User?> GetUserWithProfilePhotoAsync(int userId)
    {
        return await context.Users
            .Include(u => u.ProfilePhoto)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithBackgroundPhotoAsync(int userId)
    {
        return await context.Users
            .Include(u => u.BackgroundPhoto)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<User?> GetUserWithAllFieldsAsync(int userId)
    {
        return await context.Users
            .Include(u => u.ProfilePhoto)
            .Include(u => u.BackgroundPhoto)
            .Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    // will be used for admin panel to send notification to every user
    public async Task<List<User>> GetAllUsers()
    {
        return await context.Users.ToListAsync();
    }

    public IQueryable<User> GetUsersQueryable()
    {
        return context.Users
            .Include(u => u.ProfilePhoto)
            .Include(u => u.BackgroundPhoto);
    }

    public async Task<Dictionary<int, ChatMemberInfo>> GetUsersInfoAsync(List<int> userIds)
    {
        return await context.Users
            .Include(u => u.ProfilePhoto)
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new ChatMemberInfo
            {
                Id = u.Id,
                UserName = u.UserName!,
                PhotoUrl = u.ProfilePhoto != null ? u.ProfilePhoto.Url : null
            })
            .ToDictionaryAsync(x => x.Id, x => x);
    }

    public async Task<Dictionary<int, List<string?>>> GetUserRolesAsync(List<int> userIds)
    {
        return await context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(context.Roles, 
                ur => ur.RoleId, 
                r => r.Id, 
                (ur, r) => new { ur.UserId, r.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(
                g => g.Key, 
                g => g.Select(x => x.Name).ToList()
            );
    }
}