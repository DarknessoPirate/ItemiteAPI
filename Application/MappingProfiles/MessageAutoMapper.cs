using AutoMapper;
using Domain.DTOs.Listing;
using Domain.DTOs.Messages;
using Domain.DTOs.User;
using Domain.Entities;

namespace Application.MappingProfiles;

public class MessageAutoMapper : Profile
{
    public MessageAutoMapper()
    {
        CreateMap<Message, MessageResponse>()
            .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ListingId, opt => opt.MapFrom(src => src.ListingId))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src =>
                src.MessagePhotos.Select(mp => mp.Photo)))
            .ForMember(dest => dest.DateRead, opt => opt.MapFrom(src => src.ReadAt));

        CreateMap<Message, LastMessageInfo>()
            .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Sender.UserName))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.DateSent, opt => opt.MapFrom(src => src.DateSent));

        CreateMap<User, ChatMemberInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? "Unknown"))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.ProfilePhoto != null ? src.ProfilePhoto.Url : null));

        CreateMap<ListingBase, ListingBasicInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => 
                src.ListingPhotos.Any() 
                    ? src.ListingPhotos.OrderBy(lp => lp.Order).First().Photo.Url 
                    : string.Empty))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
            .ForMember(dest => dest.ListingType, opt => opt.MapFrom(src => 
                src is ProductListing ? "Product" : "Auction"))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => 
                src is ProductListing 
                    ? ((ProductListing)src).Price.ToString("C") 
                    : src is AuctionListing 
                        ? (((AuctionListing)src).CurrentBid ?? ((AuctionListing)src).StartingBid).ToString("C")
                        : "N/A"));
    }
}