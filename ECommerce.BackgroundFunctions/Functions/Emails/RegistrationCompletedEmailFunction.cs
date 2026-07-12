using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ECommerce.BackgroundFunctions.Messaging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class RegistrationCompletedEmailFunction(IEmailSender emailSender, ILogger<RegistrationCompletedEmailFunction> logger)
{
    private readonly ILogger<RegistrationCompletedEmailFunction> _logger = logger;
     

    [Function(nameof(RegistrationCompletedEmailFunction))]
    public async Task Run(
        [ServiceBusTrigger("%UserRegisteredQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        UserRegisteredMessage? envelope = message.Body.ToObjectFromJson<UserRegisteredMessage>() 
            ?? throw new InvalidOperationException("Unable to deserialize UserRegisteredMessage from Service Bus message.");

        UserRegisteredPayload payload = envelope.Payload;

        await emailSender.SendAsync(
            payload.Email,
            "Successful registration with ECommerce",
            message.Body.ToString(),
            cancellationToken);

        // Complete the message
        await messageActions.CompleteMessageAsync(message, cancellationToken);
    }
}
