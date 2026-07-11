namespace ECommerce.Application.Abstractions.Messaging;

public interface IMessageDispatcher
{
    ValueTask DispatchAsync(
        IMessage message,
        CancellationToken cancellationToken = default);
}
