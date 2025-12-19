using Application.Features.Banners.GetMainBanners;
using Domain.DTOs.Banners;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannerController(ISender mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<List<BannerResponse>>> GetActiveBanners()
    {
        var command = new GetActiveBannersQuery();

        var response = await mediator.Send(command);

        return Ok(response);
    }
}