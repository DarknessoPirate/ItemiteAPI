namespace Domain.DTOs.Auth;

public class RefreshTokenDTO
{
    public required string Token { get; set; }
    public DateTime TokenExpirationDate { get; set; }
}