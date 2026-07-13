using Azure;
using Azure.Communication.Email;
using Azure.Core;
using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Email;

public sealed class AzureCommunicationEmailSender(
    TokenCredential tokenCredential,
    IOptions<EmailOptions> options,
    ILogger<AzureCommunicationEmailSender> logger)
    : IEmailSender
{ 
    private readonly EmailClient _client = new(new Uri(options.Value.Endpoint),
                                                tokenCredential);     

    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage(
            senderAddress: _options.SenderAddress,
            recipients: new EmailRecipients(
            [
                new EmailAddress(to)
            ]),
            content: new EmailContent(subject)
            {
                Html = htmlBody
            });

        EmailSendOperation operation = await _client.SendAsync(
            WaitUntil.Completed,
            message,
            cancellationToken);

        logger.LogInformation(
            "Email sent. MessageId: {MessageId}",
            operation.Id);
    }
}
