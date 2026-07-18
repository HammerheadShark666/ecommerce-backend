namespace ECommerce.Infrastructure.Messaging.Azure;

public sealed class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public required string ConnectionString { get; init; }

    public required string UserRegisteredQueueName { get; init; }

    public required string PasswordResetRequestedQueueName { get; init; }

    public required string PasswordResetCompletedQueueName { get; init; }
}
