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

namespace Application.Features.Banners.AddBanner;

public class AddBannerHandler(
    UserManager<User> userManager,
    IBannerRepository bannerRepository,
    IMediaService mediaService,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<AddBannerHandler> logger
) : IRequestHandler<AddBannerCommand, BannerResponse>
{
    public async Task<BannerResponse> Handle(AddBannerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new ForbiddenException("Invalid user id");

        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(request.BannerPhoto.ContentType);
        if (fileType != FileType.Image)
            throw new BadRequestException("Only image files are allowed for banners");
        
        var dimensions = await mediaService.GetImageDimensions(request.BannerPhoto);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        string? uploadedPhotoPublicId = null;

        try
        {
            var uploadResult = await mediaService.UploadPhotoAsync(request.BannerPhoto);

            if (uploadResult.Error != null)
            {
                logger.LogError(
                    $"Banner photo upload failed: {uploadResult.Error.Message} for user ID:{request.UserId}");
                throw new BadRequestException("Banner photo upload failed");
            }

            uploadedPhotoPublicId = uploadResult.PublicId;
            
            var banner = new Banner
            {
                Name = request.Dto.Name,
                Dimensions = dimensions,
                Offset = request.Dto.Offset,
                Position = request.Dto.Position,
                IsActive = request.Dto.IsActive,
                FileName = request.BannerPhoto.FileName,
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                DateUploaded = DateTime.UtcNow
            };

            await bannerRepository.AddAsync(banner);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            
            var bannerResponse = mapper.Map<BannerResponse>(banner);

            return bannerResponse;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to add banner: {e.Message}");
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
