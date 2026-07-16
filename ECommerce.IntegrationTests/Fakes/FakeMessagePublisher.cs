using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.IntegrationTests.Fakes;

public sealed class FakeMessagePublisher : IMessagePublisher
{
    public List<IMessage> PublishedMessages { get; } = [];

    public ValueTask PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        PublishedMessages.Add(message);

        return ValueTask.CompletedTask;
    }
}
