using AutoMapper;
using Domain.DTOs.Messages;
using Domain.DTOs.Notifications;
using Domain.DTOs.Photo;
using Domain.DTOs.User;
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
    IListingRepository<ListingBase> listingRepository,
    IUserRepository userRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    INotificationService notificationService,
    ILogger<SendMessageHandler> logger
) : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // var sender = await userManager.FindByIdAsync(request.SenderId.ToString());
        var sender = await userRepository.GetUserWithProfilePhotoAsync(request.SenderId);
        if (sender == null)
            throw new BadRequestException("Sender not found");

        if (sender.Id == request.SendMessageDto.RecipientId)
            throw new BadRequestException("Can't send a message to yourself");

        var recipient = await userManager.FindByIdAsync(request.SendMessageDto.RecipientId.ToString());
        if (recipient == null)
            throw new BadRequestException("Recipient not found");

        var listing = await listingRepository.GetListingByIdAsync(request.SendMessageDto.ListingId);
        if (listing == null)
            throw new BadRequestException("Listing does not exist");
        

        if (listing.OwnerId != request.SenderId && listing.OwnerId != request.SendMessageDto.RecipientId)
            throw new BadRequestException("Either sender or recipient must be the listing owner");

        // if sender is the owner, they can only reply if somebody messaged them first
        if (listing.OwnerId == request.SenderId)
        {
            // Check if the recipient has messaged the owner first
            var hasRecipientMessagedFirst = await messageRepository.HasUserMessagedAboutListingAsync(
                request.SendMessageDto.RecipientId,  // The buyer (recipient)
                request.SenderId,                     // The owner (sender)
                request.SendMessageDto.ListingId);
            
            if (!hasRecipientMessagedFirst)
                throw new BadRequestException("You can only reply to users who have contacted you first");
        }

        // get unread messages count before sending new message
        var unreadCount = await messageRepository.GetUnreadCountAsync(
            request.SendMessageDto.ListingId,
            request.SenderId,
            request.SendMessageDto.RecipientId);
        
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        var uploadedPhotosPublicIds = new List<string>();

        try
        {
            var message = new Message
            {
                Content = request.SendMessageDto.Content,
                SenderId = request.SenderId,
                ListingId = listing.Id,
                RecipientId = request.SendMessageDto.RecipientId,
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
            

            var messageResponse = mapper.Map<MessageResponse>(message);
            
            await notificationService.NotifyMessageReceived(request.SendMessageDto.RecipientId, messageResponse);
            
            // send notification only if there were no unread messages before
            if (unreadCount == 0)
            {
                await notificationService.SendNotification([request.SendMessageDto.RecipientId], request.SenderId, new NotificationInfo
                {
                    Message = $"You received new message for listing {listing.Name} from {sender.UserName}.",
                    ListingId = request.SendMessageDto.ListingId,
                    UserId = sender.Id,
                    ResourceType = ResourceType.ChatPage.ToString(),
                    NotificationImageUrl = sender.ProfilePhoto?.Url,
                    UserInfo = new ChatMemberInfo
                    {
                        Id = sender.Id,
                        UserName = sender.UserName!,
                        PhotoUrl = sender.ProfilePhoto?.Url
                    }
                });
            }
            
            return new SendMessageResult
            {
                message = messageResponse,
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