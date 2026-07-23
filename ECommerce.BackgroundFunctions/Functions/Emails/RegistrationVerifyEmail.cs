using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Application.Constants;
using ECommerce.Application.Exceptions;
using ECommerce.BackgroundFunctions.Messaging;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.BackgroundFunctions.Functions.Emails;

public class RegistrationVerifyEmail(IECommerceDbContext dbContext,
                                     IHashSettings hashSettings,
                                     IVerificationCodeGenerator verificationCodeGenerator,
                                     IHmacsha256Hasher hmacsha256Hasher,
                                     IEmailSender emailSender,
                                     IEmailTemplateService emailTemplateService,
                                     ILogger<RegistrationCompletedFunction> logger)
{
    [Function(nameof(RegistrationVerifyEmail))]
    public async Task Run(
        [ServiceBusTrigger("%VerifyRegistrationEmailQueueName%", Connection = "AzureServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing message (Verify Registration Email Successful): {MessageId}", message.MessageId);

            VerifyRegisterationEmailPayload payload = GetPayload(message);

            (string code, string hashedCode) = GenerateAndHashCode();
            await UpdateUserAsync(payload.UserId, payload.Email, hashedCode, cancellationToken);

            await emailSender.SendAsync(
                payload.Email,
                "Registration - Verify Email",
                await GetHtmlBodyAsync(code, payload.FirstName),
                cancellationToken);

            logger.LogInformation("Verify registration email sent successfully to {Email}", payload.Email);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing verify registration email successful email message");
            throw;
        }
    }

    private (string code, string hashedCode) GenerateAndHashCode()
    {
        string code = verificationCodeGenerator.Generate();
        string hashedCode = hmacsha256Hasher.HashToken(code, RegistrationConstants.HashTypeVerifyRegistrationEmail, hashSettings.Secret);

        return (code, hashedCode);
    }
    private async Task UpdateUserAsync(Guid id, string email, string hashedCode, CancellationToken cancellationToken)
    {
        User user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.Email == email, cancellationToken)
                ?? throw new NotFoundException(nameof(User), id);

        user.EmailVerificationCode = hashedCode;
        user.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(RegistrationConstants.VerifyRegistrationEmailExpiryMinutes);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static VerifyRegisterationEmailPayload GetPayload(ServiceBusReceivedMessage message)
    {
        VerifyRegisterationEmailMessage? envelope = message.Body.ToObjectFromJson<VerifyRegisterationEmailMessage>()
                   ?? throw new InvalidOperationException("Unable to deserialize VerifyRegisterationEmailMessage from Service Bus message.");

        return envelope.Payload;
    }

    public async Task<string> GetHtmlBodyAsync(string code, string firstName) => await emailTemplateService.RenderAsync(
                EmailConstants.EmailTemplateRegistrationEmailVerification,
                new()
                {
                    ["ExpiryMinutes"] = RegistrationConstants.VerifyRegistrationEmailExpiryMinutes.ToString(),
                    ["FirstName"] = firstName,
                    ["VerificationCode"] = code,
                    ["VerifyUrl"] = UrlConstants.UrlVerifyRegistrationEmailWithCode
                });
}
