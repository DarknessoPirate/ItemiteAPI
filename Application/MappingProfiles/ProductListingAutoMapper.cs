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
                o.MapFrom(p => p.Categories));
        // TODO: map mainImageUrl from list of Images (Cloudinary service needed)
    }
}