using AutoMapper;
using Domain.Auth;
using Domain.Entities;

namespace Application.MappingProfiles;

public class UserAutoMapper : Profile
{
   public UserAutoMapper()
   {
      CreateMap<RegisterRequest, User>();
   }
}