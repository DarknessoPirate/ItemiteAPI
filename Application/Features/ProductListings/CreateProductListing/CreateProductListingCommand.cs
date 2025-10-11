using Domain.DTOs.ProductListing;
using MediatR;

namespace Application.Features.ProductListings.CreateProductListing;

public class CreateProductListingCommand : IRequest<int>
{
    public CreateProductListingRequest ProductListingDto {get; set;}
    public int UserId {get; set;}
}