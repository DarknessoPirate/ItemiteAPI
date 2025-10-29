using Application.Features.Users.ChangeBackgroundPicture;
using Application.Features.Users.ChangeEmail;
using Application.Features.Users.ChangePassword;
using Application.Features.Users.ChangeProfilePicture;
using Application.Features.Users.ConfirmEmailChange;
using Application.Features.Users.RemoveBackgroundPicture;
using Application.Features.Users.RemoveProfilePicture;
using Domain.DTOs.File;
using Domain.DTOs.User;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [Authorize]
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
        return Ok(new { PhotoUrl = photoUrl });
    }

    [Authorize]
    [HttpDelete("profile/picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var command = new RemoveProfilePictureCommand
        {
            UserId = requestContextService.GetUserId()
        };

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpPost("profile/background")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChangeBackgroundPicture(IFormFile file)
    {
        var command = new ChangeBackgroundPictureCommand
        {
            UserId = requestContextService.GetUserId(),
            File = new FileWrapper(file.FileName, file.Length, file.ContentType, file.OpenReadStream())
        };

        var photoUrl = await mediator.Send(command);
        return Ok(new { PhotoUrl = photoUrl });
    }

    [Authorize]
    [HttpDelete("profile/background")]
    public async Task<IActionResult> DeleteBackgroundPicture()
    {
        var command = new RemoveBackgroundPictureCommand
        {
            UserId = requestContextService.GetUserId()
        };

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpPut("settings/change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request)
    {
        var command = new ChangeEmailCommand
        {
            UserId = requestContextService.GetUserId(),
            changeEmailRequest = request
        };

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpGet("settings/confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token, [FromQuery] string currentEmail)
    {
        var command = new ConfirmEmailChangeCommand
        {
            UserId = requestContextService.GetUserId(),
            request = new ChangeEmailConfirmRequest
            {
                Token = token,
                CurrentEmail = currentEmail
            }
        };

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize]
    [HttpPost("settings/change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand
        {
            UserId = requestContextService.GetUserId(),
            dto = new ChangePasswordRequest
            {
                OldPassword = request.OldPassword,
                NewPassword = request.NewPassword
            }
        };

        await mediator.Send(command);

        return NoContent();
    }
}