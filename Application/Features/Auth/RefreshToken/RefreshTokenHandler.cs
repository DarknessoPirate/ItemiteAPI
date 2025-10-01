using Domain.Auth;
using Domain.Entities;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
        IJwtService jwtService,
        UserManager<User> userManager
    ) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.TokenPair.AccessToken);
        var user = await userManager.FindByNameAsync(principal.Identity.Name);

        if (user == null || user.RefreshToken != request.TokenPair.RefreshToken ||
            user.RefreshTokenExpirationDate <= DateTime.UtcNow)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }
        
        var tokenResponse = jwtService.GenerateToken(user);
        var refreshTokenResponse = jwtService.GenerateRefreshToken();
        
        user.RefreshToken = refreshTokenResponse.Token;
        user.RefreshTokenExpirationDate = refreshTokenResponse.TokenExpirationDate;

        await userManager.UpdateAsync(user);

        return new AuthResponse(user, tokenResponse, refreshTokenResponse);
    }
}