using AutoMapper;
using Domain.DTOs.Messages;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Messages.UpdateMessage;

public class UpdateMessageHandler(
    IMessageRepository messageRepository,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    INotificationService notificationService
) : IRequestHandler<UpdateMessageCommand, MessageResponse>
{
    public async Task<MessageResponse> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User does not exist");

        var message = await messageRepository.FindByIdWithPhotosAsync(request.MessageId);
        if (message == null)
            throw new BadRequestException("Message does not exist");

        if (message.SenderId != user.Id)
            throw new UnauthorizedException("You can only edit your own messages");

        if (String.IsNullOrEmpty(request.NewContent) && message.MessagePhotos.Count == 0)
            throw new BadRequestException("Cannot send an empty message without photos");

        message.Content = request.NewContent;
        message.DateModified = DateTime.UtcNow;
        messageRepository.Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var messageResponse = mapper.Map<MessageResponse>(message);
        
        await notificationService.NotifyMessageUpdated(message.RecipientId, messageResponse);
        
        return messageResponse;
    }
}