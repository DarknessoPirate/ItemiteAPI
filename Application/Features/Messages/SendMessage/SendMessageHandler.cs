using Domain.DTOs.Messages;
using Domain.DTOs.Photo;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Messages.SendMessage;

public class SendMessageHandler(
    UserManager<User> userManager,
    IMessageRepository messageRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    ILogger<SendMessageHandler> logger
) : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var sender = await userManager.FindByIdAsync(request.SenderId.ToString());
        if (sender == null)
            throw new BadRequestException("Sender not found");

        var recipient = await userManager.FindByIdAsync(request.RecipientId.ToString());
        if (recipient == null)
            throw new BadRequestException("Recipient not found");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        var uploadedPhotosPublicIds = new List<string>();

        try
        {
            var message = new Message
            {
                Content = request.Content,
                SenderId = request.SenderId,
                RecipientId = request.RecipientId,
                DateSent = DateTime.UtcNow
            };

            await messageRepository.AddAsync(message);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var photoResults = new List<PhotoUploadResult>();
            var successfulPhotos = new List<PhotoResponse>(); // easier to add to dto later instead of querying db

            foreach (var fileWrapper in request.Photos)
            {
                try
                {
                    var fileType = SupportedFileTypes.GetFileTypeFromMimeType(fileWrapper.ContentType);
                    if (fileType != FileType.Image)
                    {
                        photoResults.Add(new PhotoUploadResult
                        {
                            Success = false,
                            FileName = fileWrapper.FileName,
                            ErrorMessage = "Incorrect file type (only images are allowed)"
                        });
                        logger.LogError(
                            $"File upload failed (Incorrect format): {fileWrapper.ContentType} for user ID:{request.SenderId}");
                        continue;
                    }

                    var uploadResult = await mediaService.UploadPhotoAsync(fileWrapper);

                    if (uploadResult.Error != null)
                    {
                        photoResults.Add(new PhotoUploadResult
                        {
                            Success = false,
                            FileName = fileWrapper.FileName,
                            ErrorMessage = "Photo upload failed"
                        });
                        logger.LogError(
                            $"File upload failed: {uploadResult.Error.Message} for user ID:{request.SenderId}");
                        continue;
                    }

                    uploadedPhotosPublicIds.Add(uploadResult.PublicId);

                    var photo = new Photo
                    {
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        FileName = fileWrapper.FileName,
                        PublicId = uploadResult.PublicId,
                        DateUploaded = DateTime.UtcNow
                    };

                    await photoRepository.AddPhotoAsync(photo);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    var messagePhoto = new MessagePhoto
                    {
                        MessageId = message.Id,
                        PhotoId = photo.Id
                    };

                    message.MessagePhotos.Add(messagePhoto);

                    successfulPhotos.Add(new PhotoResponse
                    {
                        PhotoId = photo.Id,
                        Url = photo.Url
                    });

                    photoResults.Add(new PhotoUploadResult
                    {
                        Success = true,
                        FileName = fileWrapper.FileName
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed to upload photo '{fileWrapper.FileName}': {e.Message}");
                    photoResults.Add(new PhotoUploadResult
                    {
                        Success = false,
                        FileName = fileWrapper.FileName,
                        ErrorMessage = $"Unexpected server error"
                    });
                }
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return new SendMessageResult
            {
                message = new MessageResponse
                {
                    MessageId = message.Id,
                    Content = message.Content,
                    DateSent = message.DateSent,
                    DateModified = message.DateModified,
                    SenderId = message.SenderId,
                    RecipientId = message.RecipientId,
                    Photos = successfulPhotos
                },
                UploadResults = photoResults
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to send message: {e.Message}");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            // Cleanup uploaded photos from Cloudinary
            foreach (var publicId in uploadedPhotosPublicIds)
            {
                try
                {
                    await mediaService.DeleteImageAsync(publicId);
                }
                catch (Exception cleanupEx)
                {
                    logger.LogError(cleanupEx, $"Failed to cleanup photo with publicId '{publicId}'");
                }
            }

            throw;
        }
    }
}