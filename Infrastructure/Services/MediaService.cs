using System.Net.Mime;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Configs;
using Domain.DTOs.File;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using ResourceType = CloudinaryDotNet.Actions.ResourceType;

namespace Infrastructure.Services;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IOptions<CloudinarySettings> cloudinaryConfig, ILogger<MediaService> logger)
    {
        var account = new Account(cloudinaryConfig.Value.CloudName, cloudinaryConfig.Value.ApiKey,
            cloudinaryConfig.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _logger = logger;
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(FileWrapper file)
    {
        if (file == null || file.Length == 0)
        {
            throw new CloudinaryException("Empty file provided");
        }

        if (file.Length > SupportedFileTypes.MaxFileSize)
        {
            throw new CloudinaryException(
                $"File size exceeds maximum allowed size of {SupportedFileTypes.MaxFileSize / (1024 * 1024)}MB");
        }

        if (!SupportedFileTypes.IsSupportedMimeType(file.ContentType))
        {
            throw new CloudinaryException($"Unsupported file type: {file.ContentType}");
        }

        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(file.ContentType);
        if (fileType != FileType.Image)
        {
            throw new CloudinaryException("Only image files are allowed for profile photos");
        }

        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.FileStream),
                Folder = "Itemite/Images"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError($"Error while uploading image: {uploadResult.Error.Message}");
                throw new CloudinaryException($"Failed to upload photo");
            }

            return uploadResult;
        }
        catch (Exception ex) when (ex is not CloudinaryException)
        {
            _logger.LogError($"Error while uploading image: {ex.Message}");
            throw new MediaServiceException("Photo upload failed");
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
            _logger.LogError($"Error while deleting image: {ex.Message}");
            throw new MediaServiceException($"Failed to delete image");
        }
    }

    public async Task<Dimensions> GetImageDimensions(FileWrapper file)
    {
        var imageInfo = await Image.IdentifyAsync(file.FileStream);
        var dimensions = new Dimensions
        {
            Width = imageInfo.Width,
            Height = imageInfo.Height
        };

        file.FileStream.Position = 0;

        return dimensions;
    }
}