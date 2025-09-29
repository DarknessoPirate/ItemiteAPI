using Application.Features.Auth.Login;
using Application.Features.Auth.Register;
using Domain.Auth;
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
        
        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand {loginDto = request};
        var authResponse = await mediator.Send(command);
        return Ok(authResponse);
    }
}