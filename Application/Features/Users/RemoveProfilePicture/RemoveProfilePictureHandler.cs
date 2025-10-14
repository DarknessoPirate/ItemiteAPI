using Application.Features.Users.RemoveBackgroundPicture;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.RemoveProfilePicture;

public class RemoveProfilePictureHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    ILogger<RemoveBackgroundPictureHandler> logger
) : IRequestHandler<RemoveProfilePictureCommand>
{
    public async Task Handle(RemoveProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);

        if (existingUser == null)
            throw new BadRequestException("User does not exist");

        if (existingUser.ProfilePhoto == null)
            throw new BadRequestException("You don't have a profile photo to remove");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var deletionResult = await mediaService.DeleteImageAsync(existingUser.ProfilePhoto.PublicId);
            if (deletionResult.Error != null)
                throw new CloudinaryException(deletionResult.Error.Message);

            await userRepository.RemoveProfilePhotoAsync(request.UserId);
            await photoRepository.DeletePhotoAsync(existingUser.ProfilePhoto.Id);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to remove the profile photo: {e.Message}");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}