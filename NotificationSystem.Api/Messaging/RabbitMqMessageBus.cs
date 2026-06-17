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

        string dlxName = "system.dlx";
        string dlqName = "expired.queue";
        string dlqRoutingKey = "message.expired";

        await channel.ExchangeDeclareAsync(exchange: dlxName, type: ExchangeType.Direct);
        await channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queue: dlqName, exchange: dlxName, routingKey: dlqRoutingKey);

        string mainExchange = "system.exchange";
        string mainRoutingKey = "message.main";
        
        await channel.ExchangeDeclareAsync(exchange: mainExchange, type: ExchangeType.Direct);

        var queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", dlxName },
            { "x-dead-letter-routing-key", dlqRoutingKey },
            { "x-message-ttl", 15000 } 
        };

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);
        await channel.QueueBindAsync(queue: queueName, exchange: mainExchange, routingKey: mainRoutingKey);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = DeliveryModes.Persistent;

        await channel.BasicPublishAsync(mainExchange, mainRoutingKey, true, props, body);
    }
}
