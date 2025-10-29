namespace Api.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;


public static class LoggerExtensions
{
    private static readonly AnsiConsoleTheme CustomColorTheme = new(
        new Dictionary<ConsoleThemeStyle, string>
        {
            // \x1b[38;5;NNNm for text colors (where NNN is 0-255)
            // \x1b[48;5;NNNm for background colors
            [ConsoleThemeStyle.Text] = "\x1b[38;5;253m",         // Light gray
            [ConsoleThemeStyle.SecondaryText] = "\x1b[38;5;246m", // Gray
            [ConsoleThemeStyle.TertiaryText] = "\x1b[38;5;242m",  // Darker gray
            [ConsoleThemeStyle.Invalid] = "\x1b[38;5;196m",       // Red
            [ConsoleThemeStyle.Null] = "\x1b[38;5;42m",           // Green
            [ConsoleThemeStyle.Name] = "\x1b[38;5;46m",           // Bright green
            [ConsoleThemeStyle.String] = "\x1b[38;5;214m",        // Orange
            [ConsoleThemeStyle.Number] = "\x1b[38;5;199m",        // Pink
            [ConsoleThemeStyle.Boolean] = "\x1b[38;5;207m",       // Light purple
            [ConsoleThemeStyle.Scalar] = "\x1b[38;5;85m",         // Mint green
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[38;5;252m",  // Very light gray
            [ConsoleThemeStyle.LevelDebug] = "\x1b[38;5;87m",     // Cyan - DEBUG logs
            [ConsoleThemeStyle.LevelInformation] = "\x1b[38;5;39m", // Blue - INFO logs
            [ConsoleThemeStyle.LevelWarning] = "\x1b[38;5;220m",  // Yellow - WARNING logs
            [ConsoleThemeStyle.LevelError] = "\x1b[38;5;196m",    // Red - ERROR logs
            [ConsoleThemeStyle.LevelFatal] = "\x1b[38;5;196;48;5;88m" // Red on dark red - FATAL logs
        });
    
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Avoid excessive logs from ASP.NET Core internals
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information) // Log EF Core SQL queries (Redundant)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Debug, // change this to filter out the logs
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", 
                theme: CustomColorTheme
            )
            .WriteTo.File("logs/critical.log", restrictedToMinimumLevel: LogEventLevel.Fatal, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Add Serilog as the default logger
        builder.Host.UseSerilog();
    }

    

    public static void ConfigureSerilogHttpLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "\n======================== API REQUEST ========================" +
                                      "\n{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms" +
                                      "\n=============================================================\n";

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                {
                    return LogEventLevel.Error; // Log exceptions as Errors
                }

                var statusCode = httpContext.Response.StatusCode;
        
                return statusCode switch
                {
                    >= 500 => LogEventLevel.Error,    // Server errors (500+) as Error
                    >= 400 => LogEventLevel.Warning,  // Client errors (400-499) as Warning
                    _ => LogEventLevel.Information    // Everything else as Information
                };
            };
        });
    }
}