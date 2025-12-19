using Application.Exceptions;
using Domain.DTOs.Banners;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Banners.DeleteBanner;

public class DeleteBannerHandler(
    UserManager<User> userManager,
    IBannerRepository bannerRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBannerHandler> logger
) : IRequestHandler<DeleteBannerCommand>
{
    public async Task Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new ForbiddenException("Invalid user id");

        var banner = await bannerRepository.FindByIdAsync(request.BannerId);
        if (banner == null)
            throw new BadRequestException("Banner not found");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            bannerRepository.Remove(banner);
            var result = await mediaService.DeleteImageAsync(banner.PublicId);
            if (result.Error != null)
            {
                logger.LogError(result.Error.Message);
                throw new CloudinaryException("Failed to delete banner photo.");
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError($"Failed to delete banner {e.Message}");
            throw;
        }
    }
}