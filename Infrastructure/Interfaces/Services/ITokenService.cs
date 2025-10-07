using System.Security.Claims;
using Domain.Auth;
using Domain.DTOs.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces.Services;

public interface ITokenService
{
    Task<TokenPairResponse> GenerateTokenPairAsync(User user, string ipAddress, string? deviceId, string? userAgent);
    Task<(TokenPairResponse tokens, User user)> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress,
            string? deviceId, string? userAgent);
    Task RevokeTokenAsync(string token, string revokedByIp);
    Task RevokeAllUserTokensAsync(int userId, string revokedByIp);
    Task RevokeTokenChainAsync(string token, string revokedByIp);
    void SetTokensInsideCookie(TokenPairResponse tokenPair, HttpContext httpContext);
}