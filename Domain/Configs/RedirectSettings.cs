using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class RedirectSettings
{
    [Required(ErrorMessage = "RedirectSettings:StripeReturnOnboardingUrl field is required in appsettings")]
    [Url(ErrorMessage = "RedirectSettings:StripeReturnOnboardingUrl must be a valid URL in appsettings")]
    public string StripeReturnOnboardingUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "RedirectSettings:StripeRefreshOnboardingUrl field is required in appsettings")]
    [Url(ErrorMessage = "RedirectSettings:StripeRefreshOnboardingUrl must be a valid URL in appsettings")]
    public string StripeRefreshOnboardingUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "RedirectSettings:EmailVerificationUrl field is required in appsettings")]
    [Url(ErrorMessage = "RedirectSettings:EmailVerificationUrl must be a valid URL in appsettings")]
    public string EmailVerificationUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "RedirectSettings:EmailChangeConfirmationUrl field is required in appsettings")]
    [Url(ErrorMessage = "RedirectSettings:EmailChangeConfirmationUrl must be a valid URL in appsettings")]
    public string EmailChangeConfirmationUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "RedirectSettings:PasswordResetUrl field is required in appsettings")]
    [Url(ErrorMessage = "RedirectSettings:PasswordResetUrl must be a valid URL in appsettings")]
    public string PasswordResetUrl { get; set; } = string.Empty;

}