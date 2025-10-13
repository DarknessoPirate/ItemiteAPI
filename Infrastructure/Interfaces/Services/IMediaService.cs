using CloudinaryDotNet.Actions;
using Domain.DTOs.File;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces.Services;

public interface IMediaService
{
    Task<ImageUploadResult> UploadPhotoAsync(FileWrapper file);
    Task<DeletionResult> DeleteImageAsync(string publicId);
}