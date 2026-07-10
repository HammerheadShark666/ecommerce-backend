using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ECommerce.Application.Configuration;
using ECommerce.Application.Abstractions.Configuration;


namespace ECommerce.Application.Extensions;

public static class ApplicationSettings
{
    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSetting<JwtSettings, IJwtSettings>(configuration, "Jwt");
        services.AddSetting<HashSettings, IHashSettings>(configuration, "Hash");
        services.AddSetting<EncryptionSettings, IEncryptionSettings>(configuration, "Encryption");

        return services;
    }

    private static IServiceCollection AddSetting<TSettings, TInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TSettings : class, TInterface
        where TInterface : class
    {
        services.Configure<TSettings>(configuration.GetSection(sectionName));

        services.AddSingleton<TInterface>(sp =>
            sp.GetRequiredService<IOptions<TSettings>>().Value);

        return services;
    }
}
