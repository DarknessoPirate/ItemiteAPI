using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.RemoveBackgroundPicture;

public class RemoveBackgroundPictureHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    ILogger<RemoveBackgroundPictureHandler> logger
) : IRequestHandler<RemoveBackgroundPictureCommand>
{
    public async Task Handle(RemoveBackgroundPictureCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetUserWithBackgroundPhotoAsync(request.UserId);

        if (existingUser == null)
            throw new BadRequestException("User does not exist");

        if (existingUser.BackgroundPhoto == null)
            throw new BadRequestException("You don't have a background photo to remove");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var deletionResult = await mediaService.DeleteImageAsync(existingUser.BackgroundPhoto.PublicId);
            if (deletionResult.Error != null)
                throw new CloudinaryException(deletionResult.Error.Message);

            await userRepository.RemoveBackgroundPhotoAsync(request.UserId);
            await photoRepository.DeletePhotoAsync(existingUser.BackgroundPhoto.Id);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to remove the background photo: {e.Message}");
            await unitOfWork.RollbackTransactionAsync(cancellationToken); 
            throw;
        }
    }
}