using Application.Features.Users.ChangeProfilePicture;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.ChangeBackgroundPicture;

public class ChangeBackgroundPictureHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    IMediaService mediaService,
    ILogger<ChangeProfilePictureHandler> logger
) : IRequestHandler<ChangeBackgroundPictureCommand, string>
{
    public async Task<string> Handle(ChangeBackgroundPictureCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
            throw new BadRequestException("No image file provided");

        if (request.File.Length > SupportedFileTypes.MaxFileSize)
            throw new BadRequestException(
                $"File size exceeds maximum allowed size of {SupportedFileTypes.MaxFileSize / (1024 * 1024)}MB");

        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(request.File.ContentType);
        if (fileType != FileType.Image)
            throw new BadRequestException("Only image files are allowed for profile pictures");

        var existingUser = await userRepository.GetUserWithBackgroundPhotoAsync(request.UserId);
        if (existingUser == null)
            throw new BadRequestException("User not found");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        string? uploadPhotoPublicId = null;
        try
        {
            var uploadResult = await mediaService.UploadPhotoAsync(request.File);
            if (uploadResult.Error != null)
                throw new CloudinaryException(uploadResult.Error.Message);

            uploadPhotoPublicId = uploadResult.PublicId;

            // remove old photo
            if (existingUser.BackgroundPhoto != null)
            {
                var oldPhoto = existingUser.BackgroundPhoto;
                var deletionResult = await mediaService.DeleteImageAsync(oldPhoto.PublicId);
                if (deletionResult.Error != null)
                    throw new CloudinaryException(deletionResult.Error.Message);

                await userRepository.RemoveBackgroundPhotoAsync(existingUser.Id); // removes the reference to photo
                await photoRepository.DeletePhotoAsync(oldPhoto.Id); // removes photo row from db
            }

            var photo = new Photo
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                FileName = request.File.FileName,
                PublicId = uploadResult.PublicId,
                DateUploaded = DateTime.UtcNow,
            };

            await photoRepository.AddPhotoAsync(photo);
            existingUser.BackgroundPhoto = photo;
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return photo.Url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error when changing profile photo: {ex.Message}");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            if (uploadPhotoPublicId != null)
            {
                await mediaService.DeleteImageAsync(uploadPhotoPublicId);
            }

            throw;
        }
    }
}