using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Infrastructure.Messaging.Azure;

public sealed record MessageEnvelope
{
    public required Guid Id { get; init; }

    public required string Type { get; init; }

    public required DateTimeOffset OccurredOn { get; init; }

    public required object Payload { get; init; }

    public static MessageEnvelope Create<T>(T message)
        where T : IMessage => new()
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).FullName!,
            OccurredOn = DateTimeOffset.UtcNow,
            Payload = message
        };
}
