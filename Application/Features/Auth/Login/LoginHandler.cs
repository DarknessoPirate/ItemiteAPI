using Domain.Auth;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Login;

public class LoginHandler(
    IJwtService jwtService,
    UserManager<User> userManager
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

        var tokenResponse = jwtService.GenerateToken(user);
        var refreshTokenResponse = jwtService.GenerateRefreshToken();
        
        user.RefreshToken = refreshTokenResponse.Token;
        user.RefreshTokenExpirationDate = tokenResponse.TokenExpirationDate;

        await userManager.UpdateAsync(user);

        return new AuthResponse(user, tokenResponse, refreshTokenResponse);
    }
}