using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class StripeSettings
{
    [Required(ErrorMessage = "StripeSettings:SecretKey field is required in appsettings")]
    public string SecretKey { get; set; } = string.Empty;
}