
using Api.Extensions;
using Application.Extensions;
using Domain.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.HttpLogging;

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
        
        builder.Services.AddSwaggerGen();
        builder.Services.AddConfig(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.ConfigureIdentity(builder.Configuration);
        builder.Services.AddFluentEmail(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddApiServices();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; }); // setting to generate api urls in full lowercase 


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
        
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
