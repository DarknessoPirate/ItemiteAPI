using Domain.DTOs.Banners;
using MediatR;

namespace Application.Features.Banners.GetMainBanners;

public class GetActiveBannersQuery : IRequest<List<BannerResponse>>
{
    
}