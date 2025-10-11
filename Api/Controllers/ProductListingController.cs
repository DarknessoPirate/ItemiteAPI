using Application.Features.ProductListings.CreateProductListing;
using Application.Features.ProductListings.DeleteProductListing;
using Application.Features.ProductListings.GetPaginatedProductListings;
using Application.Features.ProductListings.GetProductListing;
using Application.Features.ProductListings.UpdateProductListing;
using Domain.DTOs.ProductListing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductListingController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductListings([FromQuery] GetPaginatedProductListingsQuery query)
    {
        var productListings = await mediator.Send(query);
        return Ok(productListings);
    }

    [HttpGet("{listingId}")]
    public async Task<IActionResult> GetProductListingById(int listingId)
    {
        var productListingQuery = new GetProductListingQuery {ListingId = listingId};
        var listing = await mediator.Send(productListingQuery);
        return Ok(listing);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProductListing([FromBody] CreateProductListingRequest request)
    {
        var command = new CreateProductListingCommand { ProductListingDto = request };
        var createdProductListingId = await mediator.Send(command);
        return Ok(new {createdProductListingId} );
    }

    [Authorize]
    [HttpDelete("{listingId}")]
    public async Task<IActionResult> DeleteProductListing([FromRoute] int listingId)
    {
        var command = new DeleteProductListingCommand {ListingId = listingId};
        await mediator.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpPut("{listingId}")]
    public async Task<IActionResult> UpdateProductListing([FromBody] UpdateProductListingRequest request, [FromRoute] int listingId)
    {
        var command = new UpdateProductListingCommand
        {
            UpdateDto = request,
            ListingId = listingId
        };
        var updatedProductListing = await mediator.Send(command);
        return Ok(updatedProductListing);
    }
}