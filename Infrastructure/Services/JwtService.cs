using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Auth;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtService(IOptions<JwtSettings> jwtSettings) : IJwtService
{
    
    public TokenResponse GenerateToken(User user)
    {
        var tokenExpirationInMinutes = jwtSettings.Value.TokenExpirationInMinutes;
        
        var signingCredentials = GetSigningCredentials();
        var claims = GenerateClaims(user);
        var tokenOptions = CreateTokenOptions(signingCredentials, claims, tokenExpirationInMinutes);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        var expirationDate = DateTime.Now.AddMinutes(tokenExpirationInMinutes).ToUniversalTime();

        return new TokenResponse
        {
            Token = token,
            TokenExpirationDate = expirationDate
        };
    }

    public TokenResponse GenerateRefreshToken()
    {
        var tokenExpirationInMinutes = jwtSettings.Value.RefreshTokenExpirationInMinutes;
        
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token= Convert.ToBase64String(randomNumber);
        var expirationDate = DateTime.Now.AddMinutes(tokenExpirationInMinutes).ToUniversalTime();

        return new TokenResponse
        {
            Token = token,
            TokenExpirationDate = expirationDate
        };
    }
    
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenKey = jwtSettings.Value.Key;
        
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
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var tokenKey = jwtSettings.Value.Key;
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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        return claims;
    }
    
    private JwtSecurityToken CreateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims, int minutesToExpiry)
    {
        var issuer = jwtSettings.Value.Issuer;
        var audience = jwtSettings.Value.Audience;

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