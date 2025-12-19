using AutoMapper;
using Domain.DTOs.Banners;
using Domain.Entities;

namespace Application.MappingProfiles;

public class BannerAutoMapper : Profile
{
    public BannerAutoMapper()
    {
        CreateMap<Banner, BannerResponse>();
    }
}