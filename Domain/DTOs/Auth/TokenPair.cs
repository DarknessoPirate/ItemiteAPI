using Domain.Auth;
using Domain.Entities;

namespace Domain.DTOs.Auth;

public class TokenPair
{
    public AccessTokenDTO AccessToken { get; set; }
    public RefreshTokenDTO RefreshToken { get; set; }
}