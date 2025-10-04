namespace Domain.Configs;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenExpirationInMinutes { get; set; } = 1440;
    public int RefreshTokenExpirationInMinutes { get; set; } = 10080;
}