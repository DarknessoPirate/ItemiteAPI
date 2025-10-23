using AutoMapper;
using Domain.DTOs.ProductListing;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.MappingProfiles;

public class ProductListingAutoMapper : Profile
{
    public ProductListingAutoMapper()
    {
        CreateMap<CreateProductListingRequest, ProductListing>()
            .ForMember(dest => dest.Location, opt => 
                opt.MapFrom(src => IsLocationComplete(src.Location) ? src.Location : null));

        CreateMap<ProductListing, ProductListingResponse>()
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
               && !string.IsNullOrWhiteSpace(location.State);
    }
}