using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationSystem.Api.Messaging;
using RabbitMQ.Client;

public class RabbitMqMessageBus : IRabbitMqMessageBus
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqMessageBus(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    public async Task Publish<T>(T message, string queueName)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            Port = int.Parse(_settings.Port),
            UserName = _settings.Username,
            Password = _settings.Password
        };

        var endpoints = new System.Collections.Generic.List<AmqpTcpEndpoint> {
            new AmqpTcpEndpoint("hostname"),
            new AmqpTcpEndpoint("rabbitmq", int.Parse(_settings.Port))
        };

        IConnection conn = await factory.CreateConnectionAsync(endpoints);
        IChannel channel = await conn.CreateChannelAsync(); 

        await channel.QueueDeclareAsync(queueName, false, false, false, null);
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = DeliveryModes.Persistent;

        await channel.BasicPublishAsync("", queueName, true, props, body);
    }
}
