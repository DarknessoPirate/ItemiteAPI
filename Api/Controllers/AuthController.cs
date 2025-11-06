using Application.Features.Auth.EmailConfirmation;
using Application.Features.Auth.Login;
using Application.Features.Auth.LoginGoogle;
using Application.Features.Auth.LoginGoogleCallback;
using Application.Features.Auth.Logout;
using Application.Features.Auth.LogoutFromAllDevices;
using Application.Features.Auth.RefreshToken;
using Application.Features.Auth.Register;
using Application.Features.Auth.ResetPassword;
using Domain.Auth;
using Domain.DTOs.Auth;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("login/google")]
    public async Task<IActionResult> LoginGoogle([FromQuery] string returnUrl)
    {
        var command = new LoginGoogleCommand {ReturnUrl = returnUrl};
        var authProperties = await mediator.Send(command);
        return Challenge(authProperties, "Google");
    }
    
    [HttpGet("login/google/callback", Name = "LoginGoogleCallback")]
    public async Task<IActionResult> LoginGoogleCallback([FromQuery] string returnUrl)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }
        
        var command = new LoginGoogleCallbackCommand
        {
            ReturnUrl = returnUrl,
            ClaimsPrincipal = result.Principal,
            IpAddress = requestContextService.GetIpAddress(),
            DeviceId = requestContextService.GetDeviceId(),
            UserAgent = requestContextService.GetUserAgent()
        };
        
        await mediator.Send(command);
        
        return Redirect(command.ReturnUrl);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        var command = new RefreshTokenCommand
        {
            IpAddress = requestContextService.GetIpAddress(),
            DeviceId = requestContextService.GetDeviceId(),
            UserAgent = requestContextService.GetUserAgent()
        };

        var response = await mediator.Send(command);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand
        {
            IpAddress = requestContextService.GetIpAddress()
        };
        
        var successMessage = await mediator.Send(command);
        return Ok(new {successMessage});
    }

    [Authorize]
    [HttpPost("logout-all-devices")]
    public async Task<IActionResult> LogoutFromAllDevices()
    {
        var command = new LogoutFromAllDevicesCommand
        {
            IpAddress = requestContextService.GetIpAddress()
        };
        
        var successMessage = await mediator.Send(command);
        return Ok(new {successMessage});
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