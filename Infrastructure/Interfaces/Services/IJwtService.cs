using System.Security.Claims;
using Domain.Auth;
using Domain.Entities;

namespace Infrastructure.Interfaces.Services;

public interface IJwtService
{
    TokenResponse GenerateToken(User user);
    TokenResponse GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}