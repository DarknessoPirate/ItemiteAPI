using Application.Exceptions;
using AutoMapper;
using Domain.DTOs.Banners;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Banners.UpdateBanner;

public class UpdateBannerHandler(
    UserManager<User> userManager,
    IBannerRepository bannerRepository,
    IMediaService mediaService,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<UpdateBannerHandler> logger
) : IRequestHandler<UpdateBannerCommand, BannerResponse>
{
    public async Task<BannerResponse> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new ForbiddenException("Invalid user id");

        var banner = await bannerRepository.FindByIdAsync(request.BannerId);
        if (banner == null)
            throw new NotFoundException($"Banner with ID {request.BannerId} not found");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        string? uploadedPhotoPublicId = null;
        string? oldPhotoPublicId = null;

        try
        {
            if (request.BannerPhoto != null)
            {
                var fileType = SupportedFileTypes.GetFileTypeFromMimeType(request.BannerPhoto.ContentType);
                if (fileType != FileType.Image)
                    throw new BadRequestException("Only image files are allowed for banners");

                var dimensions = await mediaService.GetImageDimensions(request.BannerPhoto);
                var uploadResult = await mediaService.UploadPhotoAsync(request.BannerPhoto);

                if (uploadResult.Error != null)
                {
                    logger.LogError(
                        $"Banner photo upload failed: {uploadResult.Error.Message} for user ID:{request.UserId}");
                    throw new BadRequestException("Banner photo upload failed");
                }

                uploadedPhotoPublicId = uploadResult.PublicId;
                oldPhotoPublicId = banner.PublicId;

                await mediaService.DeleteImageAsync(oldPhotoPublicId);

                banner.Dimensions = dimensions;
                banner.FileName = request.BannerPhoto.FileName;
                banner.Url = uploadResult.SecureUrl.AbsoluteUri;
                banner.PublicId = uploadResult.PublicId;
                banner.DateUploaded = DateTime.UtcNow;
            }

            banner.Name = request.Dto.Name;
            banner.Offset = request.Dto.Offset;
            banner.Position = request.Dto.Position;
            banner.IsActive = request.Dto.IsActive;

            bannerRepository.Update(banner);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var bannerResponse = mapper.Map<BannerResponse>(banner);
            return bannerResponse;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to update banner: {e.Message}");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            if (!string.IsNullOrEmpty(uploadedPhotoPublicId))
            {
                try
                {
                    await mediaService.DeleteImageAsync(uploadedPhotoPublicId);
                }
                catch (Exception cleanupEx)
                {
                    logger.LogError(cleanupEx,
                        $"Failed to cleanup banner photo with publicId '{uploadedPhotoPublicId}'");
                }
            }

            throw;
        }
    }
}