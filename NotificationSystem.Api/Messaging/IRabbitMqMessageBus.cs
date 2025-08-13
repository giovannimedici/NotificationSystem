namespace NotificationSystem.Api.Messaging;

public interface IRabbitMqMessageBus
{
    Task Publish<T>(T message, string queueName);
}
