using AutoMapper;
using Domain.DTOs.Category;
using Domain.Entities;

namespace Application.MappingProfiles;

public class CategoryAutoMapper : Profile
{
    public CategoryAutoMapper()
    {
        CreateMap<CreateCategoryRequest, Category>();
    }
}