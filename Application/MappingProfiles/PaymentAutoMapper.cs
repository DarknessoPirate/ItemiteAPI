using AutoMapper;
using Domain.DTOs.Payments;
using Domain.Entities;

namespace Application.MappingProfiles;

public class PaymentAutoMapper : Profile
{
    public PaymentAutoMapper()
    {
        CreateMap<Payment, PaymentResponse>()
            .ForMember(dest => dest.PaymentId, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Listing, opt =>
                opt.MapFrom(src => src.Listing))
            .ForMember(dest => dest.Buyer, opt =>
                opt.MapFrom(src => src.Buyer))
            .ForMember(dest => dest.Seller, opt =>
                opt.MapFrom(src => src.Seller))
            .ForMember(dest => dest.ApprovedBy, opt =>
                opt.MapFrom(src => src.ApprovedBy))
            .ForMember(dest => dest.Dispute, opt =>
                opt.MapFrom(src => src.Dispute));


        CreateMap<Payment, PaymentBuyerResponse>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src.Listing))
            .ForMember(dest => dest.Buyer, opt => opt.MapFrom(src => src.Buyer))
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller))
            .ForMember(dest => dest.Dispute, opt => opt.MapFrom(src => src.Dispute));
        
        CreateMap<Payment, PaymentSellerResponse>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src.Listing))
            .ForMember(dest => dest.Buyer, opt => opt.MapFrom(src => src.Buyer))
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller));

    }
}