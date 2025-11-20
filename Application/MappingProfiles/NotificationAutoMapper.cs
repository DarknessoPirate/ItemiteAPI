using AutoMapper;
using Domain.DTOs.Notifications;
using Domain.Entities;

namespace Application.MappingProfiles;

public class NotificationAutoMapper : Profile
{
    public NotificationAutoMapper()
    {
        CreateMap<NotificationInfo, Notification>()
            // to prevent AutoMapper from setting date time to -infinite
            .ForMember(dest => dest.NotificationSent, opt => opt.Ignore());
        CreateMap<Notification, NotificationInfo>()
            .ForMember(dest => dest.NotificationId, o => o.MapFrom(src => src.Id))
            .ForMember(dest => dest.ReadAt, opt => opt.MapFrom((src, dest, destMember, context) =>
                src.NotificationUsers.FirstOrDefault(u => u.UserId == (int)context.Items["UserId"])!.ReadAt
            ));
    }
}