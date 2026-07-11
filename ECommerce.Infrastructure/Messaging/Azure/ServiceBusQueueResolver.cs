using ECommerce.Application.Features.Registration.Events;
using ECommerce.Infrastructure.Messaging.Azure.Interface;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Messaging.Azure;

public sealed class ServiceBusQueueResolver(IOptions<AzureServiceBusOptions> options)
    : IServiceBusQueueResolver
{
    public string GetQueueName(Type messageType)
    {
        if (messageType == typeof(UserRegistered))
        { 
            return options.Value.UserRegisteredQueueName;
        }

        throw new InvalidOperationException(
            $"No queue configured for {messageType.Name}");
    }
}
