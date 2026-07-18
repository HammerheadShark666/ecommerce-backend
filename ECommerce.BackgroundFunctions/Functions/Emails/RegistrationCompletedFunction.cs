using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Email;
using ECommerce.BackgroundFunctions.Messaging;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class RegistrationCompletedFunction(IEmailSender emailSender,
                                           IEmailTemplateService emailTemplateService,
                                           ILogger<RegistrationCompletedFunction> logger)
{ 
    [Function(nameof(RegistrationCompletedFunction))]
    public async Task Run(
        [ServiceBusTrigger("%UserRegisteredQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing message (Registration Successful): {MessageId}", message.MessageId);

            UserRegisteredMessage? envelope = message.Body.ToObjectFromJson<UserRegisteredMessage>() 
            ?? throw new InvalidOperationException("Unable to deserialize UserRegisteredMessage from Service Bus message.");

            UserRegisteredPayload payload = envelope.Payload;

            string htmlBody = await emailTemplateService.RenderAsync(
                EmailConstants.EmailTemplateRegistrationSuccessful, 
                new() {
                    ["Name"] = payload.FirstName,
                    ["LoginUrl"] = UrlConstants.UrlLogin
                });

            await emailSender.SendAsync(
                payload.Email,
                "Successful registration with ECommerce",
                htmlBody,
                cancellationToken);

            logger.LogInformation("Registration successful email sent successfully to {Email}", payload.Email);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing registration successful email message");
            throw;
        }
    }
}
