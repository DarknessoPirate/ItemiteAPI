using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class CloudinarySettings
{
    [Required(ErrorMessage = "CloudinarySettings:CloudName field is required in appsettings")]
    public string CloudName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CloudinarySettings:ApiKey field is required in appsettings")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "CloudinarySettings:ApiSecret field is required in appsettings")]
    public string ApiSecret { get; set; } = string.Empty;
}