using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ECommerce.Application.Configuration;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;


namespace ECommerce.Application.Extensions;

public static class ApplicationSettings
{
    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSetting<JwtSettings, IJwtSettings>(configuration, SettingsConstants.JwtSectionName);
        services.AddSetting<HashSettings, IHashSettings>(configuration, SettingsConstants.HashSectionName);
        services.AddSetting<EncryptionSettings, IEncryptionSettings>(configuration, SettingsConstants.EncryptionSectionName);

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
