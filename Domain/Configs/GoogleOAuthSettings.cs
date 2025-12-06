using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class GoogleOAuthSettings
{
    [Required(ErrorMessage = "GoogleOAuth:ClientId field is required in appsettings")]
    public string ClientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "GoogleOAuth:ClientSecret field is required in appsettings")]
    public string ClientSecret { get; set; } = string.Empty;
}