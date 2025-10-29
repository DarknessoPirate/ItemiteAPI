using AutoMapper;
using Domain.DTOs.Photo;
using Domain.Entities;

namespace Application.MappingProfiles;

public class PhotoAutoMapper : Profile
{
    public PhotoAutoMapper()
    {
        CreateMap<Photo, PhotoResponse>()
            .ForMember(dest => dest.PhotoId, opt => opt.MapFrom(src => src.Id));
    }
}