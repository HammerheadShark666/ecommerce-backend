using System.Security.Cryptography;
using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Application.Constants;
using ECommerce.BackgroundFunctions.Messaging;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class PasswordResetRequestedEmail(IECommerceDbContext dbContext,
                                         IEmailSender emailSender,
                                         IEmailTemplateService emailTemplateService,
                                         IHmacsha256Hasher hmacsha256Hasher,
                                         IHashSettings hashSettings,
                                         ILogger<PasswordResetRequestedEmail> logger)
{
    [Function(nameof(PasswordResetRequestedEmail))]
    public async Task Run(
        [ServiceBusTrigger("%PasswordResetRequestedQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing message (password reset request): {MessageId}", message.MessageId);

            PasswordResetRequestMessage? envelope = message.Body.ToObjectFromJson<PasswordResetRequestMessage>()
            ?? throw new InvalidOperationException("Unable to deserialize PasswordResetRequestMessage from Service Bus message.");

            PasswordResetRequestPayload payload = envelope.Payload;
                         
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            string hashedPasswordResetToken = hmacsha256Hasher.HashToken(token, AuthenticationConstants.HashTypeTokenPasswordReset, hashSettings.Secret);
             
            await MarkExistingTokensAsUsedAsync(cancellationToken);
            await CreatePasswordResetTokenAsync(payload.UserId, hashedPasswordResetToken, cancellationToken);
             
            //Cleanup job: a scheduled task(or just filter WHERE ExpiresAt > UtcNow in your queries) to purge expired rows periodically, otherwise the table grows forever
             

            string htmlBody = await emailTemplateService.RenderAsync(
                EmailConstants.EmailTemplatePasswordResetRequest,
                new()
                {
                    ["Name"] = payload.FirstName, 
                    ["PasswordResetUrl"] = UrlConstants.UrlPasswordReset,
                    ["ResetToken"] = token,                    
                    ["ExpiryTime"] = "30 minutes"
                });

            await emailSender.SendAsync(
                payload.Email,
                "Password Reset",
                htmlBody,
                cancellationToken);

            logger.LogInformation("Password reset request email sent successfully to {Email}", payload.Email);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing password reset request  email message");
            throw;
        }
    }

    private async Task CreatePasswordResetTokenAsync(Guid userId, string tokenHash, CancellationToken cancellationToken)
    {
        var passwordResetToken = new PasswordResetToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedByIp = ""
        };

        dbContext.PasswordResetTokens.Add(passwordResetToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkExistingTokensAsUsedAsync(CancellationToken cancellationToken) => await dbContext.PasswordResetTokens
            .Where(t => !t.Used && t.ExpiresAt < DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Used, true)
                .SetProperty(t => t.UsedAt, DateTime.UtcNow), cancellationToken);
}
