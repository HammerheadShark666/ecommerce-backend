using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BackgroundFunctions.Extensions;

public static class DependencyInjectionExtension
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, AzureCommunicationEmailSender>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    }
}
