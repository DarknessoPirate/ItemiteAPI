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
            options.AddPolicy("FrontendClient", policybuilder =>
                    policybuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(configuration["AllowedOrigins"] ?? throw new ConfigException("AllowedOrigin field missing from appsettings"))
                        .AllowCredentials() // cookies 
            );
        });
    }
}