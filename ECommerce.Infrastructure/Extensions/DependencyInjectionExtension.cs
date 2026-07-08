using Microsoft.Extensions.DependencyInjection;
using ECommerce.Core.Abstractions;
using ECommerce.Infrastructure.Background_Jobs;
using ECommerce.Infrastructure.Library;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Services.Intefaces;

namespace ECommerce.Infrastructure.Extensions;

public static class DependencyInjectionExtension
{
    public static void AddInterfaceClassExtension(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IOneTimePasswordGenerator, OneTimePasswordGenerator>();
        services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
        services.AddScoped<IAesEncryptionHelper, AesEncryptionHelper>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IHmacsha256Hasher, Hmacsha256Hasher>();
        services.AddScoped<ITokenCleanupService, TokenCleanupService>();

        services.AddHostedService<TokenCleanupJob>();
    }
}
