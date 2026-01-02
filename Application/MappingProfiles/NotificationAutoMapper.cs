using AutoMapper;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;

namespace Application.MappingProfiles;

public class NotificationAutoMapper : Profile
{
    public NotificationAutoMapper()
    {
        CreateMap<NotificationInfo, Notification>()
            // to prevent AutoMapper from setting date time to -infinite
            .ForMember(dest => dest.NotificationSent, opt => opt.Ignore())
            .ForMember(dest => dest.ResourceType,
                opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ResourceType)
                        ? Enum.Parse<ResourceType>(src.ResourceType)
                        : (ResourceType?)null));
        CreateMap<Notification, NotificationInfo>()
            .ForMember(dest => dest.NotificationId, o => o.MapFrom(src => src.Id))
            .ForMember(dest => dest.ReadAt, opt => opt.MapFrom((src, dest, destMember, context) =>
                src.NotificationUsers.FirstOrDefault(u => u.UserId == (int)context.Items["UserId"])!.ReadAt
            ))
            .ForMember(dest => dest.ResourceType,
                opt => opt.MapFrom(src => src.ResourceType != null
                    ? src.ResourceType.ToString()
                    : null));
    }
}