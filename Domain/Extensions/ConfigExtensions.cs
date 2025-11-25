using Domain.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Extensions;

public static class ConfigExtensions
{
    public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthSettings>()
            .Bind(configuration.GetSection("AuthSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection("SmtpSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<GoogleOAuthSettings>()
            .Bind(configuration.GetSection("GoogleOAuth"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<CloudinarySettings>()
            .Bind(configuration.GetSection("CloudinarySettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<StripeSettings>()
            .Bind(configuration.GetSection("StripeSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<RedirectSettings>()
            .Bind(configuration.GetSection("RedirectSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<PaymentSettings>()
            .Bind(configuration.GetSection("PaymentSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<SeedSettings>(configuration.GetSection("SeedSettings"));
    }
}