using ECommerce.Application.Features.ForgottenPassword.Events;
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
        else if (messageType == typeof(PasswordResetRequested))
        {
            return options.Value.PasswordResetRequestedQueueName;
        }
        else if (messageType == typeof(PasswordResetCompleted))
        {
            return options.Value.PasswordResetCompletedQueueName;
        } 

        throw new InvalidOperationException(
            $"No queue configured for {messageType.Name}");
    }
}
