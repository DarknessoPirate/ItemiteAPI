using System.Text;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Extensions;

public static class IdentityExtensions
{
    public static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettngs = configuration.GetSection("Jwt");
        var authSettings = configuration.GetSection("AuthSettings");
        var googleOAuthSettings = configuration.GetSection("GoogleOAuth");

        services.AddIdentity<User, IdentityRole<int>>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 7;
                    // options.Password.RequiredUniqueChars = 1; // (1 means a password like "aaaaaaaaa" would be allowed if only this rule was set up)

                    // lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // username settings
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = true;
                }
            )
            .AddEntityFrameworkStores<ItemiteDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(authSettings.GetValue<int>("EmailTokenLifespanInMinutes"));
        });
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie().AddGoogle(options =>
            {
                var clientId = googleOAuthSettings.GetValue<string>("ClientId") ??
                               throw new ConfigException("ClientId missing in appsettings.json");
                var clientSecret = googleOAuthSettings.GetValue<string>("ClientSecret") ??
                                   throw new ConfigException("ClientSecret missing in appsettings.json");

                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettngs["Issuer"],
                        ValidAudience = jwtSettngs["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettngs["Key"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Cookies["accessToken"];

                            //if no cookie, checks query string (for SignalR connections)
                            if (string.IsNullOrEmpty(token))
                            {
                                var accessToken = context.Request.Query["access_token"];
                                var path = context.HttpContext.Request.Path;

                                // only accepts query string tokens for SignalR hubs
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    path.StartsWithSegments("/hubs"))
                                {
                                    token = accessToken;
                                }
                            }

                            context.Token = token;
                            return Task.CompletedTask;
                        }
                    };
                }
            );
    }
}