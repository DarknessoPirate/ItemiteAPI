using Domain.Auth;
using Domain.Entities;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
        ITokenService tokenService,
        UserManager<User> userManager
    ) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (tokens,user) = await tokenService.RefreshTokenAsync(
            request.TokenPair.AccessToken,
            request.TokenPair.RefreshToken,
            request.IpAddress,
            request.DeviceId,
            request.UserAgent
        );
        
        return new AuthResponse(user, tokens);
    }
}