using AutoMapper;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Reports.CreateReport;

public class CreateReportHandler(
    IReportRepository reportRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    UserManager<User> userManager,
    ILogger<CreateReportHandler> logger
    ) : IRequestHandler<CreateReportCommand, int>
{
    public async Task<int> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var report = mapper.Map<Report>(request.ReportDto);
        report.UserId = request.UserId;
        
        var savedPhotosPublicIds = new List<string>();
        var reportPhotos = new List<ReportPhoto>();
        
        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    var uploadResult = await mediaService.UploadPhotoAsync(image);
                    if (uploadResult.Error != null)
                    {
                        foreach (var savedPhoto in savedPhotosPublicIds)
                        {
                            await mediaService.DeleteImageAsync(savedPhoto);
                        }

                        throw new CloudinaryException(uploadResult.Error.Message);
                    }

                    savedPhotosPublicIds.Add(uploadResult.PublicId);
                    var photo = new Photo
                    {
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        PublicId = uploadResult.PublicId,
                        FileName = image.FileName
                    };
                    await photoRepository.AddPhotoAsync(photo);

                    var reportPhoto = new ReportPhoto
                    {
                        Photo = photo,
                    };
                    reportPhotos.Add(reportPhoto);
                }
                report.ReportPhotos = reportPhotos;
            }
            await reportRepository.AddAsync(report);
            await unitOfWork.CommitTransactionAsync();
        }  
        catch (Exception ex)
        {
            foreach (var savedPhotos in savedPhotosPublicIds)
            {
                await mediaService.DeleteImageAsync(savedPhotos);
            }

            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error when creating new report: {ex.Message}");
            throw;
        }
        
        return report.Id;
    }
}