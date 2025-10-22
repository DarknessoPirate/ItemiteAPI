using AutoMapper;
using Domain.DTOs.Listing;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.MappingProfiles;

public class ListingAutoMapper : Profile
{
    public ListingAutoMapper()
    {
        CreateMap<ListingBase, ListingBasicResponse>()
            .ForMember(p => p.Categories, o =>
                o.MapFrom(p => p.Categories))
            .ForMember(p => p.MainImageUrl, o =>
                o.MapFrom(p => p.ListingPhotos.FirstOrDefault(p => p.Order == 1).Photo.Url))
            .ForMember(p => p.Location, opt => 
                opt.MapFrom(src => IsLocationComplete(src.Location) ? src.Location : null))
            .ForMember(dest => dest.ListingType, opt => opt.MapFrom(src => "Unknown"))
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.IsNegotiable, opt => opt.Ignore())
            .ForMember(dest => dest.StartingBid, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentBid, opt => opt.Ignore())
            .Include<ProductListing, ListingBasicResponse>()
            .Include<AuctionListing, ListingBasicResponse>();
        
        CreateMap<ProductListing, ListingBasicResponse>()
            .ForMember(dest => dest.ListingType, opt => opt.MapFrom(src => "Product"))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.IsNegotiable, opt => opt.MapFrom(src => src.IsNegotiable))
            .ForMember(dest => dest.StartingBid, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentBid, opt => opt.Ignore());

        CreateMap<AuctionListing, ListingBasicResponse>()
            .ForMember(dest => dest.ListingType, opt => opt.MapFrom(src => "Auction"))
            .ForMember(dest => dest.StartingBid, opt => opt.MapFrom(src => src.StartingBid))
            .ForMember(dest => dest.CurrentBid, opt => opt.MapFrom(src => src.CurrentBid))
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.IsNegotiable, opt => opt.Ignore());
    }
    
    private bool IsLocationComplete(Location? location)
    {
        if (location == null) return false;
        
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.State);
    }
}