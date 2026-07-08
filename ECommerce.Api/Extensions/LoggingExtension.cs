using Serilog;

namespace ECommerce.Api.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddApplicationLogging(
        this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()); 

        return builder;
    }
}
