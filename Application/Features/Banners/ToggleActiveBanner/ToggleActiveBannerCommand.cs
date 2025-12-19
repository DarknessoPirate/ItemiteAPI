using Domain.DTOs.Banners;
using MediatR;

namespace Application.Features.Banners.ToggleActiveBanner;

public class ToggleActiveBannerCommand : IRequest<BannerResponse>
{
    public int UserId { get; set; }
    public int BannerId { get; set; }
}