using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Infrastructure.Messaging.Azure.Interface;

namespace ECommerce.Infrastructure.Messaging.Azure;

public sealed class AzureServiceBusMessagePublisher(
    ServiceBusClient client,
    IServiceBusQueueResolver resolver)
        : IMessagePublisher
{ 
    public async ValueTask PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        var queueName = resolver.GetQueueName(typeof(TMessage));
        var _sender = client.CreateSender(queueName);

        await _sender.SendMessageAsync(
           CreateMessage(message),
           cancellationToken);
    }

    private static ServiceBusMessage CreateMessage<TMessage>(TMessage message)
    where TMessage : IMessage
    {
        var envelope = MessageEnvelope.Create(message);
        var json = JsonSerializer.Serialize(envelope);

        return new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = envelope.Type,
            MessageId = envelope.Id.ToString()
        };
    }
}
