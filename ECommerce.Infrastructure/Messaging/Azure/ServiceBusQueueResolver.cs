using ECommerce.Application.Features.ForgottenPassword.Events;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Infrastructure.Messaging.Azure.Interface;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Messaging.Azure;

public sealed class ServiceBusQueueResolver(IOptions<AzureServiceBusOptions> options) : IServiceBusQueueResolver
{ 
    private static readonly IReadOnlyDictionary<Type, Func<AzureServiceBusOptions, string>> QueueMappings =
        new Dictionary<Type, Func<AzureServiceBusOptions, string>>
        {
            { typeof(UserRegistered), o => o.UserRegisteredQueueName },
            { typeof(PasswordResetRequested), o => o.PasswordResetRequestedQueueName },
            { typeof(PasswordResetCompleted), o => o.PasswordResetCompletedQueueName },
            { typeof(VerifyRegistrationEmail), o => o.VerifyRegistrationEmailQueueName }
        };
  
    public string GetQueueName(Type messageType)
    {
        if (QueueMappings.TryGetValue(messageType, out Func<AzureServiceBusOptions, string>? selector))
        {
            return selector(options.Value);
        }

        throw new ArgumentOutOfRangeException(
            nameof(messageType),
            messageType,
            $"No queue mapping exists for message type '{messageType.Name}'.");
    }
}
