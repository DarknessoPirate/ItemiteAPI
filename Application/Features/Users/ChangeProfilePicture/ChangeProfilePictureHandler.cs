using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.ChangeProfilePicture;

public class ChangeProfilePictureHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    IMediaService mediaService
) : IRequestHandler<ChangeProfilePictureCommand, string>
{
    public async Task<string> Handle(ChangeProfilePictureCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
            throw new BadRequestException("No image file provided", []);

        if (request.File.Length > SupportedFileTypes.MaxFileSize)
            throw new BadRequestException(
                $"File size exceeds maximum allowed size of {SupportedFileTypes.MaxFileSize / (1024 * 1024)}MB");

        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(request.File.ContentType);
        if (fileType != FileType.Image)
            throw new BadRequestException("Only image files are allowed for profile pictures");

        var existingUser = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
        if (existingUser == null)
            throw new NotFoundException("User not found");

        var uploadResult = await mediaService.UploadPhotoAsync(request.File);
        if (uploadResult.Error != null)
            throw new CloudinaryException(uploadResult.Error.Message);

        // remove old photo
        if (existingUser.ProfilePhoto != null)
        {
            var photoId = existingUser.ProfilePhoto.Id;
            var deletionResult = await mediaService.DeleteImageAsync(existingUser.ProfilePhoto.PublicId);
            if (deletionResult.Error != null)
                throw new CloudinaryException(deletionResult.Error.Message);

            await userRepository.RemoveProfilePhotoAsync(existingUser.Id); // removes the reference to photo
            await photoRepository.DeletePhotoAsync(photoId); // removes photo row from db
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var photo = new Photo
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            DateUploaded = DateTime.UtcNow,
        };

        await photoRepository.AddPhotoAsync(photo);
        existingUser.ProfilePhoto = photo;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return photo.Url;
    }
}