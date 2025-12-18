using AutoMapper;
using Domain.DTOs.Category;
using Domain.Entities;

namespace Application.MappingProfiles;

public class CategoryAutoMapper : Profile
{
    public CategoryAutoMapper()
    {
        CreateMap<CreateCategoryRequest, Category>();
        CreateMap<Category, CategoryResponse>()
            .ForMember(c => c.ImageUrl, opt =>
                opt.MapFrom(cr => cr.Photo.Url));
        CreateMap<Category, CategoryTreeResponse>()
            .ForMember(c => c.SubCategories, opt => opt.Ignore())
            .ForMember(c => c.ImageUrl, opt =>
            opt.MapFrom(cr => cr.Photo.Url));
        CreateMap<Category, CategoryBasicResponse>();
    }
}