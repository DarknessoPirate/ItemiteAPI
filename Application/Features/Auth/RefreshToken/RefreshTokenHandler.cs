using AutoMapper;
using Domain.DTOs.Auth;
using Domain.DTOs.User;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;


namespace Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
        ITokenService tokenService,
        IHttpContextAccessor contextAccessor,
        IMapper mapper
    ) : IRequestHandler<RefreshTokenCommand, UserBasicResponse>
{
    public async Task<UserBasicResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
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
        
        // var authResponse = new AuthResponse(user, tokens);
            
        tokenService.SetTokensInsideCookie(tokens, httpContext);

        return mapper.Map<UserBasicResponse>(user);
    }
}