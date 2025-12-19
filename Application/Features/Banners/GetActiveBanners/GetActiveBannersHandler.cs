using AutoMapper;
using Domain.DTOs.Banners;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Banners.GetMainBanners;

public class GetActiveBannersHandler(
    IBannerRepository bannerRepository,
    IMapper mapper
    
    ) : IRequestHandler<GetActiveBannersQuery, List<BannerResponse>>
{
    public async Task<List<BannerResponse>> Handle(GetActiveBannersQuery request, CancellationToken cancellationToken)
    {
        var banners = await bannerRepository.FindAllActiveAsync();

        return mapper.Map<List<BannerResponse>>(banners);
    }
}