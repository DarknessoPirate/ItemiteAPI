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
}