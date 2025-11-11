using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Auth;
using Domain.Configs;
using Domain.DTOs.Auth;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService(
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings
    ) : ITokenService
{
    public async Task<TokenPair> GenerateTokenPairAsync(User user, string ipAddress, string? deviceId,
        string? userAgent)
    {
        var accessToken = GenerateJwtToken(user);
        var jwtId = new JwtSecurityTokenHandler().ReadJwtToken(accessToken.Token).Id;

        var refreshToken = await CreateRefreshTokenAsync(user, jwtId, ipAddress, deviceId, userAgent);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = new RefreshTokenDTO
            {
                Token = refreshToken.Token,
                TokenExpirationDate = refreshToken.ExpiresAt
            }
        };
    }

    // this also returns the user, either it returns it here or we will have to parse the token to get the id and fetch the user again in the handler
    // this is cursed but the other solution is even more cursed imo
    public async Task<(TokenPair tokens, User user)> RefreshTokenAsync(string expiredAccessToken,
        string refreshToken,
        string ipAddress,
        string? deviceId, string? userAgent)
    {
        var principal = GetPrincipalFromExpiredToken(expiredAccessToken);
        var jwtId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jwtId))
        {
            throw new InvalidTokenException("Invalid access token - missing JTI claim");
        }

        var storedRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedRefreshToken == null)
        {
            throw new InvalidTokenException("Invalid refresh token");
        }

        // validate the refresh token
        if (storedRefreshToken.JwtId != jwtId)
        {
            throw new InvalidTokenException("Token mismatch - JTI does not match");
        }

        if (storedRefreshToken.IsExpired)
        {
            throw new InvalidTokenException("Refresh token has expired");
        }

        if (storedRefreshToken.IsRevoked)
        {
            await RevokeTokenChainAsync(refreshToken, ipAddress);
            throw new InvalidTokenException("Token reuse detected, all tokens have been revoked");
        }

        var user = await userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var newAccessToken = GenerateJwtToken(user);
        var newJwtId = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken.Token).Id;

        var newRefreshToken = await CreateRefreshTokenAsync(user, newJwtId, ipAddress, deviceId, userAgent);
        storedRefreshToken.IsRevoked = true;
        storedRefreshToken.RevokedAt = DateTime.UtcNow;
        storedRefreshToken.RevokedByIp = ipAddress;
        storedRefreshToken.ReplacedByTokenId = newRefreshToken.Id;

        refreshTokenRepository.Update(storedRefreshToken);
        await unitOfWork.SaveChangesAsync();

        var tokens = new TokenPair
        {
            AccessToken = newAccessToken,
            RefreshToken = new RefreshTokenDTO
            {
                Token = newRefreshToken.Token,
                TokenExpirationDate = newRefreshToken.ExpiresAt
            }
        };

        return (tokens, user);
    }

    public async Task RevokeTokenAsync(string token, string revokedByIp)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new NotFoundException("Token not found");

        if (!refreshToken.IsActive)
            throw new BadRequestException("Token is not active", []);

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;

        refreshTokenRepository.Update(refreshToken);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId, string revokedByIp)
    {
        var activeTokens = await refreshTokenRepository.GetAllActiveTokensForUserAsync(userId);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            refreshTokenRepository.Update(token);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task RevokeTokenChainAsync(string token, string revokedByIp)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new NotFoundException("Refresh token not found");

        var tokenChain = await refreshTokenRepository.GetTokenChainAsync(refreshToken.Id);

        foreach (var tkn in tokenChain)
        {
            if (tkn.IsActive)
            {
                tkn.IsRevoked = true;
                tkn.RevokedAt = DateTime.UtcNow;
                tkn.RevokedByIp = revokedByIp;
                refreshTokenRepository.Update(tkn);
            }
        }

        await unitOfWork.SaveChangesAsync();
    }

    public void SetTokensInsideCookie(TokenPair tokenPair, HttpContext httpContext)
    {
        var accessToken = tokenPair.AccessToken;
        var refreshToken = tokenPair.RefreshToken;
        
        httpContext.Response.Cookies.Append("accessToken", accessToken.Token, new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Expires = refreshToken.TokenExpirationDate,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        
        httpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            Path = "/",
            Expires = refreshToken.TokenExpirationDate,
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }

    public void RemoveTokensFromCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("accessToken", new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });

        httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(User user, string jwtId, string createdByIp,
        string? deviceId, string? userAgent)
    {
        var generatedRefreshToken = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = generatedRefreshToken.Token,
            JwtId = jwtId,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = generatedRefreshToken.TokenExpirationDate,
            CreatedByIp = createdByIp,
            DeviceId = deviceId,
            UserAgent = userAgent
        };

        await refreshTokenRepository.CreateAsync(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return refreshToken;
    }

    private AccessTokenDTO GenerateJwtToken(User user)
    {
        var tokenExpirationInMinutes = jwtSettings.Value.AccessTokenExpirationInMinutes;

        var signingCredentials = GetSigningCredentials();
        var claims = GenerateClaims(user);
        var tokenOptions = CreateTokenOptions(signingCredentials, claims, tokenExpirationInMinutes);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        var expirationDate = DateTime.Now.AddMinutes(tokenExpirationInMinutes).ToUniversalTime();

        return new AccessTokenDTO
        {
            Token = token,
            TokenExpirationDate = expirationDate
        };
    }

    private RefreshTokenDTO GenerateRefreshToken()
    {
        var tokenExpirationInMinutes = jwtSettings.Value.RefreshTokenExpirationInMinutes;

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        var expirationDate = DateTime.Now.AddMinutes(tokenExpirationInMinutes).ToUniversalTime();

        return new RefreshTokenDTO
        {
            Token = token,
            TokenExpirationDate = expirationDate
        };
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenKey = jwtSettings.Value.Key ?? throw new ConfigException("Cannot access token key from appsettings.json");
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid access token");

        return principal;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var tokenKey = jwtSettings.Value.Key ?? throw new ConfigException("Cannot access token key from appsettings.json");
        
        if (tokenKey.Length < 64)
            throw new ConfigException("Token key needs to be at least 64 characters long");

        var key = Encoding.UTF8.GetBytes(tokenKey);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private List<Claim> GenerateClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!)
        };
        return claims;
    }

    private JwtSecurityToken CreateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims,
        int minutesToExpiry)
    {
        var issuer = jwtSettings.Value.Issuer ?? throw new ConfigException("Cannot access issuer from appsettings.json");
        var audience = jwtSettings.Value.Audience ?? throw new ConfigException("Cannot access audience from appsettings.json");
        
        var tokenOptions = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(minutesToExpiry),
            signingCredentials: signingCredentials
        );

        return tokenOptions;
    }
}