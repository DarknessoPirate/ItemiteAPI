using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Repositories;

public class PhotoRepository(ItemiteDbContext context) : IPhotoRepository
{
    public async Task AddPhotoAsync(Photo photo)
    {
        await context.Photos.AddAsync(photo); 
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        var photo = await context.Photos.FindAsync(photoId);
        if (photo != null)
            context.Photos.Remove(photo);
    }
}