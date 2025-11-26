using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Messages.DeleteMessage;

public class DeleteMessageHandler(
    UserManager<User> userManager,
    IListingRepository<ListingBase> listingRepository,
    IMessageRepository messageRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    ILogger<DeleteMessageHandler> logger
)
    : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User does not exist");

        var message = await messageRepository.FindByIdWithPhotosAsync(request.MessageId);
        if (message == null)
            throw new BadRequestException("Message does not exist");
        if (message.SenderId != user.Id)
            throw new BadRequestException("You can only delete your own messages");
        if (message.IsDeleted)
            throw new BadRequestException("Message is already deleted");
        

        var listing = await listingRepository.GetListingByIdAsync(message.ListingId);
        if (listing.IsArchived)
            throw new BadRequestException("Cannot delete archived messages");

        message.Content = "User deleted the message";
        message.IsDeleted = true;
        messageRepository.Update(message);

        var photosToDelete = message.MessagePhotos.Select(mp => mp.Photo).ToList();

        foreach (var photo in photosToDelete)
        {
            try
            {
                var result = await mediaService.DeleteImageAsync(photo.PublicId);
                if (result.Error == null)
                {
                    await photoRepository.DeletePhotoAsync(photo.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to delete photo {photo.Id}");
            }
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await notificationService.NotifyMessageDeleted(message.RecipientId, message.Id, message.Content);
    }
}