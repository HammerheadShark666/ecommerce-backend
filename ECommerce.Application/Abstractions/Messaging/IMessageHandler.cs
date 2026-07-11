namespace ECommerce.Application.Abstractions.Messaging;

public interface IMessageHandler<in TMessage>
    where TMessage : IMessage
{
    ValueTask Handle(
        TMessage message,
        CancellationToken cancellationToken);
}
