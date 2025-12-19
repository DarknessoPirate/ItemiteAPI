using Domain.DTOs.Banners;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Banners.UpdateBanner;

public class UpdateBannerCommand : IRequest<BannerResponse>
{
    public int UserId { get; set; }
    public int BannerId { get; set; }
    public UpdateBannerRequest Dto { get; set; }
    public FileWrapper? BannerPhoto { get; set; }
}