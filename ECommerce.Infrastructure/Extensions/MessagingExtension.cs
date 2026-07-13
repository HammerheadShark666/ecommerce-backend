using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Infrastructure.Messaging.Azure;
using ECommerce.Infrastructure.Messaging.Azure.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Extensions;

public static class MessagingExtension
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(
            configuration.GetSection(AzureServiceBusOptions.SectionName));

        services.AddSingleton<ServiceBusClient>(sp =>
        {
            IOptions<AzureServiceBusOptions> options =
                sp.GetRequiredService<IOptions<AzureServiceBusOptions>>();

            return new ServiceBusClient(
                options.Value.ConnectionString);
        });

        services.AddSingleton<IMessagePublisher, AzureServiceBusMessagePublisher>();
        services.AddSingleton<IServiceBusQueueResolver, ServiceBusQueueResolver>();

        return services;
    }
}
