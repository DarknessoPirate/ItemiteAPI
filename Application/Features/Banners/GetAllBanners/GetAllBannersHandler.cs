using Application.Exceptions;
using AutoMapper;
using Domain.DTOs.Banners;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Banners.GetAllBanners;

public class GetAllBannersHandler(
    UserManager<User> userManager,
    IBannerRepository bannerRepository,
    IMapper mapper
    ) : IRequestHandler<GetAllBannersQuery, List<BannerResponse>>
{
    public async Task<List<BannerResponse>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new ForbiddenException("Invalid user id");

        var banners = await bannerRepository.FindAllAsync();

        return mapper.Map<List<BannerResponse>>(banners);
    }
}