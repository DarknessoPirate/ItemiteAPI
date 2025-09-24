
using Api.Extensions;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.HttpLogging;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.ConfigureSerilog();

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
        });
        
        builder.Services.AddSwaggerGen();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddApiServices();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; }); // setting to generate api urls in full lowercase 


        var app = builder.Build();

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
        
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
