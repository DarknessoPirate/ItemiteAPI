using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class JwtSettings
{
    [Required(ErrorMessage = "JwtSettings:Issuer field is required in appsettings")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Audience field is required in appsettings")]
    public string Audience { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Key field is required in appsettings")]
    [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters")]
    public string Key { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "JwtSettings:AccessTokenExpirationInMinutes field must be greater than 0 in appsettings")]
    public int AccessTokenExpirationInMinutes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "JwtSettings:RefreshTokenExpirationInMinutes field must be greater than 0 in appsettings")]
    public int RefreshTokenExpirationInMinutes { get; set; }
}