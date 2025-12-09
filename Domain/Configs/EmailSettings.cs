using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class EmailSettings
{
    [Required(ErrorMessage = "EmailSettings:DefaultFromEmail field is required in appsettings")]
    [EmailAddress(ErrorMessage = "EmailSettings:DefaultFromEmail must be a valid email address in appsettings")]
    public string DefaultFromEmail { get; set; } = string.Empty;
}