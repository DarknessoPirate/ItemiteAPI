using Application.Features.ProductListings.CreateProductListing;
using Domain.DTOs.ProductListing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductListingController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProductListing([FromBody] CreateProductListingRequest request)
    {
        var command = new CreateProductListingCommand { ProductListingDto = request };
        var createdProductListingId = await mediator.Send(command);
        return Ok(new {createdProductListingId} );
    }
}