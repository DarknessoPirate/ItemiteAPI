using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class SmtpSettings
{
    [Required(ErrorMessage = "SmtpSettings:Host field is required in appsettings")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SmtpSettings:Port must be between 1 and 65535 in appsettings")]
    public int Port { get; set; }

    [Required(ErrorMessage = "SmtpSettings:Username field is required in appsettings")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "SmtpSettings:Password field is required in appsettings")]
    public string Password { get; set; } = string.Empty;
}