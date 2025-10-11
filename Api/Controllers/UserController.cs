using Application.Features.Users.ChangeProfilePicture;
using Domain.DTOs.File;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost("profile/picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChangeProfilePicture(IFormFile file)
    {
        var command = new ChangeProfilePictureCommand
        {
            UserId = requestContextService.GetUserId(),
            File = new FileWrapper(file.FileName, file.Length, file.ContentType, file.OpenReadStream())
        };
        
        var photoUrl = await mediator.Send(command);
        return Ok(new {PhotoUrl = photoUrl});
    }
}