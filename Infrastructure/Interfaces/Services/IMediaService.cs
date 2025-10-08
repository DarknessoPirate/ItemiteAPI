using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces.Services;

public interface IMediaService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeleteImageAsync(string publicId);
}