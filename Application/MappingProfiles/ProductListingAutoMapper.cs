using AutoMapper;
using Domain.DTOs.ProductListing;
using Domain.Entities;

namespace Application.MappingProfiles;

public class ProductListingAutoMapper : Profile
{
    public ProductListingAutoMapper()
    {
        CreateMap<CreateProductListingRequest, ProductListing>();
    }
}