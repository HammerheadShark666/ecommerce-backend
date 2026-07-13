namespace ECommerce.Infrastructure.Messaging.Azure.Interface;

public interface IServiceBusQueueResolver
{
    string GetQueueName(Type messageType);
}
