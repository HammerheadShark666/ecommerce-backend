using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class RegistrationCompletedEmailFunction(ILogger<RegistrationCompletedEmailFunction> logger)
{
    private readonly ILogger<RegistrationCompletedEmailFunction> _logger = logger;
     

    [Function(nameof(RegistrationCompletedEmailFunction))]
    public async Task Run(
        [ServiceBusTrigger("%UserRegisteredQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}
