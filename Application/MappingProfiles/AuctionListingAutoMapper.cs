using AutoMapper;
using Domain.DTOs.AuctionListing;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.MappingProfiles;

public class AuctionListingAutoMapper : Profile
{
    public AuctionListingAutoMapper()
    {
        CreateMap<CreateAuctionListingRequest, AuctionListing>()
            .ForMember(dest => dest.Location, opt =>
                opt.MapFrom(src => IsLocationComplete(src.Location) ? src.Location : null))
            .ForMember(dest => dest.DateEnds, opt =>
                opt.MapFrom(src => src.DateEnds ?? DateTime.UtcNow.AddDays(15)));
    }
    
    private bool IsLocationComplete(Location? location)
    {
        if (location == null) return false;
        
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.PostalCode);
    }
}