using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Configurations;

namespace ECommerce.Infrastructure.Extensions;

public static class ConfigurationExtensions
{ 
    public static IServiceCollection AddTypedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddAllOptions(this IServiceCollection services,IConfiguration configuration)
    { 
        services.AddTypedOptions<EncryptionOptions>(configuration, EncryptionOptions.Section);
        services.AddTypedOptions<HashOptions>(configuration, HashOptions.Section);
        services.AddTypedOptions<JwtOptions>(configuration, JwtOptions.Section);
        services.AddTypedOptions<EmailOptions>(configuration, EmailOptions.Section);

        return services;
    }
}
