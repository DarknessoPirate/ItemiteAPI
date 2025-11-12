using Api.Middlewares;
using Infrastructure.Exceptions;
using Serilog;

namespace Api.Extensions;

public static class ApiExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddSingleton(Log.Logger);
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                                 throw new ConfigException("AllowedOrigin field missing from appsettings");

            options.AddPolicy("FrontendClient", policybuilder =>
                policybuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );
        });
    }
}