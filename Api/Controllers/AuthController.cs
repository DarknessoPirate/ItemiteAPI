using Application.Features.Auth.EmailConfirmation;
using Application.Features.Auth.Login;
using Application.Features.Auth.RefreshToken;
using Application.Features.Auth.Register;
using Application.Features.Auth.ResetPassword;
using Domain.Auth;
using Domain.DTOs.Auth;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ForgotPasswordRequest = Microsoft.AspNetCore.Identity.Data.ForgotPasswordRequest;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand {registerDto = request};
        var userId = await mediator.Send(command);
        
        return Created($"api/user/{userId}",new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            loginDto = request,
            IpAddress = requestContextService.GetIpAddress(),
            DeviceId = requestContextService.GetDeviceId(),
            UserAgent = requestContextService.GetUserAgent()
        };

        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] TokenPairRequest request)
    {
        var command = new RefreshTokenCommand
        {
            TokenPair = request,
            IpAddress = requestContextService.GetIpAddress(),
            DeviceId = requestContextService.GetDeviceId(),
            UserAgent = requestContextService.GetUserAgent()
        };

        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> EmailConfirmation([FromQuery] string email, [FromQuery] string token)
    {
        var command = new EmailConfirmationCommand
        {
            EmailConfirmationRequest = new EmailConfirmationRequest
            {
                Email = email,
                Token = token
            }
        };
        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand
        {
            forgotPasswordDto = request
        };
        
        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            resetPasswordRequest = request
        };
        
        await mediator.Send(command);
        return Ok();
    }
}