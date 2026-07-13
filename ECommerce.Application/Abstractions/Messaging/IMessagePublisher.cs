namespace ECommerce.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    ValueTask PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage;
}
