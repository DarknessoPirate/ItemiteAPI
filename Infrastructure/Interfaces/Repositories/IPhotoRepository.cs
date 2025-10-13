using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IPhotoRepository
{
    Task AddPhotoAsync(Photo photo);
    Task DeletePhotoAsync(int photoId);
}