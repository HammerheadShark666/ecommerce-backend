using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Background_Jobs;
using ECommerce.Infrastructure.Email;
using ECommerce.Infrastructure.Library;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Services.Intefaces;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IEmailSender, AzureCommunicationEmailSender>();

        services.AddHostedService<TokenCleanupJob>();
    }
}
