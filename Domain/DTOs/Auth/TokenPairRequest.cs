namespace Domain.DTOs.Auth;

public class TokenPairRequest
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}