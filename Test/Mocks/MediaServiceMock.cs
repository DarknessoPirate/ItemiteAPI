using CloudinaryDotNet.Actions;
using Domain.DTOs.File;
using Infrastructure.Interfaces.Services;

namespace Test.Mocks;

public class MediaServiceMock : IMediaService
{
    public Task<ImageUploadResult> UploadPhotoAsync(FileWrapper file)
    {
        var uploadResult = new ImageUploadResult
        {
            PublicId = $"fake-public-id-{Guid.NewGuid()}",
            SecureUrl = new Uri($"https://fake-cloudinary-url.com/{file.FileName}"),
            Error = null
        };

        return Task.FromResult(uploadResult);
    }

    public Task<DeletionResult> DeleteImageAsync(string publicId)
    {
        var deletionResult = new DeletionResult
        {
            Result = "ok"
        };

        return Task.FromResult(deletionResult);
    }
}