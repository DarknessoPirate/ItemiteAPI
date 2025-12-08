using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class AuthSettings
{
    [Required(ErrorMessage = "AuthSettings:IsEmailConfirmationRequired field is required in appsettings")]
    public bool IsEmailConfirmationRequired { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "AuthSettings:EmailTokenLifespanInMinutes must be greater than 0 in appsettings")]
    public int EmailTokenLifespanInMinutes { get; set; }
}