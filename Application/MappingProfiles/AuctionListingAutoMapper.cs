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
        
        CreateMap<AuctionListing, AuctionListingResponse>()
            .ForMember(p => p.Categories, o =>
                o.MapFrom(p => p.Categories))
            .ForMember(p => p.Owner, o =>
                o.MapFrom(p => p.Owner))
            .ForMember(p => p.MainImageUrl, o =>
                o.MapFrom(p => p.ListingPhotos.FirstOrDefault(p => p.Order == 1).Photo.Url))
            .ForMember(p => p.Location, opt => 
                opt.MapFrom(src => IsLocationComplete(src.Location) ? src.Location : null));
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