using AutoMapper;
using Domain.DTOs.Messages;
using Domain.Entities;

namespace Application.MappingProfiles;

public class MessageAutoMapper : Profile
{
    public MessageAutoMapper()
    {
        CreateMap<Message, MessageResponse>()
            .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ListingId, opt => opt.MapFrom(src => src.ListingId))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => 
                src.MessagePhotos.Select(mp => mp.Photo)));
    }
}
