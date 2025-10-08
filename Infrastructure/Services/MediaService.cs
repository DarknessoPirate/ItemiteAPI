using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Configs;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;

    public MediaService(IOptions<CloudinarySettings> cloudinaryConfig)
    {
        var account = new Account(cloudinaryConfig.Value.CloudName, cloudinaryConfig.Value.ApiKey,
            cloudinaryConfig.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new CloudinaryException("Empty file provided");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "Itemite/Images"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new CloudinaryException($"Failed to upload photo: {uploadResult.Error.Message}");
            }

            return uploadResult;
        }
        catch (Exception ex) when (ex is not CloudinaryException)
        {
            // only wrap non-cloudinaryExceptions
            throw new MediaServiceException($"Photo upload failed: {ex.Message}");
        }
    }

    public async Task<DeletionResult> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
        {
            throw new CloudinaryException("Image public ID cannot be null or empty");
        }


        var deleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        try
        {
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        catch (Exception ex)
        {
            throw new MediaServiceException($"Failed to delete media: {ex.Message}");
        }
    }
}