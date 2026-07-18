using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BackgroundFunctions.Extensions;

public static  class OptionsExtension
{
    public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.Section));
        services.Configure<UrlOptions>(configuration.GetSection(UrlOptions.Section));
        services.Configure<HashOptions>(configuration.GetSection(HashOptions.Section));
    }
}
