using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Email;
using ECommerce.BackgroundFunctions.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class RegistrationCompletedEmailFunction(IEmailSender emailSender,
                                                IEmailTemplateService emailTemplateService,
                                                ILogger<RegistrationCompletedEmailFunction> logger)
{
    private readonly ILogger<RegistrationCompletedEmailFunction> _logger = logger;
     

    [Function(nameof(RegistrationCompletedEmailFunction))]
    public async Task Run(
        [ServiceBusTrigger("%UserRegisteredQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    { 
        UserRegisteredMessage? envelope = message.Body.ToObjectFromJson<UserRegisteredMessage>() 
            ?? throw new InvalidOperationException("Unable to deserialize UserRegisteredMessage from Service Bus message.");

        UserRegisteredPayload payload = envelope.Payload;

        string htmlBody = await emailTemplateService.RenderAsync(
            "RegistrationSuccessful", 
            new() {
                ["Name"] = payload.FirstName,
                ["LoginUrl"] = "/login"
            });

        await emailSender.SendAsync(
            payload.Email,
            "Successful registration with ECommerce",
            htmlBody,
            cancellationToken);
         
        await messageActions.CompleteMessageAsync(message, cancellationToken);
    }
}
