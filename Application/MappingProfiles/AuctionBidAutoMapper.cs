using AutoMapper;
using Domain.DTOs.AuctionListing;
using Domain.Entities;

namespace Application.MappingProfiles;

public class AuctionBidAutoMapper : Profile
{
    public AuctionBidAutoMapper()
    {
        CreateMap<AuctionBid, AuctionBidResponse>();
    }
}