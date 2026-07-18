using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Email;
using ECommerce.BackgroundFunctions.Messaging;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class PasswordResetCompleted(IEmailSender emailSender,
                                    IEmailTemplateService emailTemplateService,
                                    ILogger<PasswordResetCompleted> logger)
{
     

    [Function(nameof(PasswordResetCompleted))]
    public async Task Run(
        [ServiceBusTrigger("%PasswordResetCompletedQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing message (Password reset completed Successful): {MessageId}", message.MessageId);

            PasswordResetCompletedMessage? envelope = message.Body.ToObjectFromJson<PasswordResetCompletedMessage>()
            ?? throw new InvalidOperationException("Unable to deserialize UserRegisteredMessage from Service Bus message.");

            PasswordResetCompletedPayload payload = envelope.Payload;

            string htmlBody = await emailTemplateService.RenderAsync(
                EmailConstants.EmailTemplatePasswordResetCompletedRequest,
                new()
                {
                    ["Name"] = payload.FirstName,
                    ["ChangedDateTime"] = payload.UpdatedAt.ToString(),
                    ["LoginUrl"] = UrlConstants.UrlLogin
                });

            await emailSender.SendAsync(
                payload.Email,
                "Successful password change with ECommerce",
                htmlBody,
                cancellationToken);

            logger.LogInformation("Password reset completed  successful email sent successfully to {Email}", payload.Email);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing Password reset completed successful email message");
            throw;
        }
    }
}
