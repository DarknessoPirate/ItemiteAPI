using Application.Exceptions;
using AutoMapper;
using Domain.DTOs.Banners;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Banners.ToggleActiveBanner;

public class ToggleActiveBannerHandler(
    UserManager<User> userManager,
    IBannerRepository bannerRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<ToggleActiveBannerCommand, BannerResponse>
{
    public async Task<BannerResponse> Handle(ToggleActiveBannerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new ForbiddenException("Invalid user id");

        var banner = await bannerRepository.FindByIdAsync(request.BannerId);
        if (banner == null)
            throw new BadRequestException("Banner not found");

        banner.IsActive = !banner.IsActive;
        bannerRepository.Update(banner);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<BannerResponse>(banner);
    }
}