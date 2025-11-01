
using System.Text.Json.Serialization;
using Api.Extensions;
using Application.Extensions;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.ConfigureSerilog();

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
        });
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.MapType<SortBy>(() => new OpenApiSchema
            {
                Type = "string",
                Enum = Enum.GetNames(typeof(SortBy))
                    .Select(name => new OpenApiString(name))
                    .Cast<IOpenApiAny>()
                    .ToList()
            });

            c.MapType<SortDirection>(() => new OpenApiSchema
            {
                Type = "string",
                Enum = Enum.GetNames(typeof(SortDirection))
                    .Select(name => new OpenApiString(name))
                    .Cast<IOpenApiAny>()
                    .ToList()
            });
        });
        builder.Services.AddConfig(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.ConfigureIdentity(builder.Configuration);
        builder.Services.AddFluentEmail(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddApiServices();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddOpenApi();
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; }); // setting to generate api urls in full lowercase 
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendClient", policybuilder =>
                policybuilder.AllowAnyMethod().AllowAnyHeader().WithOrigins(builder.Configuration["AllowedOrigins"])
            );
        });

        var app = builder.Build();
        
        
        // Seed the database
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var seeder = services.GetRequiredService<IDatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler(_ => { });
        app.ConfigureSerilogHttpLogging();
        app.UseHttpsRedirection();
        app.UseHttpsRedirection();
        app.UseCors("FrontendClient");
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
