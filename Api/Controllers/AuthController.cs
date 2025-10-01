using Application.Features.Auth.EmailConfirmation;
using Application.Features.Auth.Login;
using Application.Features.Auth.RefreshToken;
using Application.Features.Auth.Register;
using Domain.Auth;
using Domain.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand {registerDto = request};
        var userId = await mediator.Send(command);
        
        return Created($"api/user/{userId}",new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand {loginDto = request};
        var authResponse = await mediator.Send(command);
        return Ok(authResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenPairRequest request)
    {
        var command = new RefreshTokenCommand {TokenPair = request};
        var authResponse = await mediator.Send(command);
        return Ok(authResponse);
    }

    [HttpGet("emailconfirmation")]
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
}