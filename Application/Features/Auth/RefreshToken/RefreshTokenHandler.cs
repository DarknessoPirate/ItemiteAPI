using Domain.Auth;
using Domain.DTOs.Auth;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;


namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
        ITokenService tokenService,
        IHttpContextAccessor contextAccessor
    ) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var httpContext = contextAccessor.HttpContext!;
        
        httpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
        httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        var tokenPair = new TokenPairRequest
        {
            AccessToken = accessToken ?? throw new UnauthorizedException("Access token not found in cookies"),
            RefreshToken = refreshToken ?? throw new UnauthorizedException("Refresh token not found in cookies")
        };
        
        var (tokens,user) = await tokenService.RefreshTokenAsync(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            request.IpAddress,
            request.DeviceId,
            request.UserAgent
        );
        
        var authResponse = new AuthResponse(user, tokens);
            
        tokenService.SetTokensInsideCookie(authResponse, httpContext);

        return authResponse;
    }
}