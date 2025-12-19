using Domain.DTOs.Banners;
using MediatR;

namespace Application.Features.Banners.GetAllBanners;

public class GetAllBannersQuery : IRequest<List<BannerResponse>>
{
    public int UserId { get; set; }
}