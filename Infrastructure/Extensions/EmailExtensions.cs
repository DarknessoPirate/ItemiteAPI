using System.Net;
using System.Net.Mail;
using Infrastructure.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class EmailExtensions
{
    public static void AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultFromEmail = configuration["EmailSettings:DefaultFromEmail"];
        var host = configuration["SMTPSetting:Host"];
        var port = configuration.GetValue<int>("SMTPSetting:Port");
        var username = configuration["SMTPSetting:Username"]; 
        var password = configuration["SMTPSetting:Password"];

        services.AddFluentEmail(defaultFromEmail)
            .AddSmtpSender(() => new SmtpClient(host)
            {
                Port = port,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            })
            .AddRazorRenderer();

        services.AddScoped<IEmailService, EmailService>();
    }
}