using Domain.DTOs.Banners;
using Domain.DTOs.File;
using MediatR;

namespace Application.Features.Banners.AddBanner;

public class AddBannerCommand : IRequest<BannerResponse>
{
    public AddBannerRequest Dto { get; set; }
    public int UserId { get; set; }
    public FileWrapper BannerPhoto { get; set; }
    
}