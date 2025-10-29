namespace Domain.Auth;

public class AccessTokenDTO
{
    public required string Token { get; set; }
    public DateTime TokenExpirationDate { get; set; }
}