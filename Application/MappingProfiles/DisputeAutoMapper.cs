using AutoMapper;
using Domain.DTOs.Payments;
using Domain.Entities;

namespace Application.MappingProfiles;

public class DisputeAutoMapper : Profile
{
    public DisputeAutoMapper()
    {
        CreateMap<Dispute, DisputeResponse>()
            .ForMember(dest => dest.Listing, opt =>
                opt.MapFrom(src => src.Payment.Listing))
            .ForMember(dest => dest.DisputedBy, opt =>
                opt.MapFrom(src => src.InitiatedBy))
            .ForMember(dest => dest.Notes, opt =>
                opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.Evidence, opt =>
            opt.MapFrom(src => src.Evidence.Select(e => e.Photo).ToList()));
    }
}