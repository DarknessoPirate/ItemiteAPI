using AutoMapper;
using Domain.DTOs.Reports;
using Domain.Entities;

namespace Application.MappingProfiles;

public class ReportsAutoMapper : Profile
{
    public ReportsAutoMapper()
    {
        CreateMap<CreateReportRequest, Report>();
        CreateMap<Report, ReportResponse>()
            .ForMember(r => r.ImagesUrls, o =>
                o.MapFrom(src => src.ReportPhotos.Select(rp => rp.Photo.Url).ToList()))
            .ForMember(r => r.ReportSubmitter, o =>
                o.MapFrom(src => src.User))
            .ForMember(r => r.ReportedResource, o =>
                o.MapFrom(src => src.ResourceType));

    }
}