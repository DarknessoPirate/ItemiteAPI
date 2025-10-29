using AutoMapper;
using Domain.Auth;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.MappingProfiles;

public class UserAutoMapper : Profile
{
   public UserAutoMapper()
   {
      CreateMap<RegisterRequest, User>()
         .ForMember(dest => dest.Location, opt => 
            opt.MapFrom(src => IsLocationComplete(src.Location) ? src.Location : null));
      CreateMap<User, UserBasicResponse>();
      CreateMap<User, UserResponse>()
         .ForMember(u => u.PhotoUrl, o =>
            o.MapFrom(u => u.ProfilePhoto.Url))
         .ForMember(u => u.Location, opt => 
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