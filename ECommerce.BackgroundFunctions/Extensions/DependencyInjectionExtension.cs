using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Email;
using ECommerce.Infrastructure.Library;
using ECommerce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BackgroundFunctions.Extensions;

public static class DependencyInjectionExtension
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, AzureCommunicationEmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IECommerceDbContext, ECommerceDbContext>();
        services.AddScoped<IHmacsha256Hasher, Hmacsha256Hasher>();
    }
}
