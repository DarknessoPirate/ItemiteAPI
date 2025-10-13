using AutoMapper;
using Domain.DTOs.ProductListing;
using Domain.Entities;

namespace Application.MappingProfiles;

public class ProductListingAutoMapper : Profile
{
    public ProductListingAutoMapper()
    {
        CreateMap<CreateProductListingRequest, ProductListing>();
        CreateMap<ProductListing, ProductListingBasicResponse>()
            .ForMember(p => p.Categories, o =>
                o.MapFrom(p => p.Categories))
            .ForMember(p => p.MainImageUrl, o =>
                o.MapFrom(p => p.ListingPhotos.FirstOrDefault(p => p.Order == 1).Photo.Url));

        CreateMap<ProductListing, ProductListingResponse>()
            .ForMember(p => p.Categories, o =>
                o.MapFrom(p => p.Categories))
            .ForMember(p => p.Owner, o =>
                o.MapFrom(p => p.Owner))
            .ForMember(p => p.MainImageUrl, o =>
                o.MapFrom(p => p.ListingPhotos.FirstOrDefault(p => p.Order == 1).Photo.Url));
    }
}