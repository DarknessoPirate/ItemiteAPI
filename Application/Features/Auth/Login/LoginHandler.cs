using Domain.Auth;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Login;

public class LoginHandler(
    ITokenService tokenService,
    UserManager<User> userManager,
    IConfiguration configuration
    ) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.loginDto.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!await userManager.CheckPasswordAsync(user, request.loginDto.Password))
        {
            throw new UnauthorizedException("Invalid password");
        }
        
        // checks the appsettings.json if email confirmation is needed to log in
        var isEmailConfRequired = configuration.GetValue<bool>("AuthSettings:IsEmailConfirmationRequired");
        
        if (isEmailConfRequired)
        {
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                throw new UnauthorizedException("Email is not confirmed");
            }
        }

        var tokens = await tokenService.GenerateTokenPairAsync(
            user,
            request.IpAddress,
            request.DeviceId,
            request.UserAgent
        );
        
        return new AuthResponse(user, tokens);
    }
}