namespace Domain.Auth;

public class TokenResponse
{
    public required string Token { get; set; }
    public DateTime TokenExpirationDate { get; set; }
}