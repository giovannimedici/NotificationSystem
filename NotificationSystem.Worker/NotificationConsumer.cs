using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationSystem.Worker;

public class NotificationConsumer : BackgroundService
{
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly RabbitMqSettings _settings;

    public NotificationConsumer(ILogger<NotificationConsumer> logger, IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Connecting to RabbitMQ at {_settings.Hostname}:{_settings.Port}");
        var factory = new ConnectionFactory
        {
            HostName = _settings.Hostname,
            Port = int.Parse(_settings.Port),
            UserName = _settings.Username,
            Password = _settings.Password
        };

        var endpoints = new System.Collections.Generic.List<AmqpTcpEndpoint> {
            new AmqpTcpEndpoint("localhost"),
            new AmqpTcpEndpoint("rabbitmq", int.Parse(_settings.Port))
        };
        
        var connection = await factory.CreateConnectionAsync(endpoints);
        IChannel channel = await connection.CreateChannelAsync();

        var queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "system.dlx" },
            { "x-dead-letter-routing-key", "message.expired" },
            { "x-message-ttl", 15000 } 
        };
        await channel.QueueDeclareAsync(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (ch, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<UserCreatedEvent>(json);

                    _logger.LogInformation($"Email sent to {@event.Email}");
                };

        await channel.BasicConsumeAsync(queue: _settings.QueueName, autoAck: true, consumer: consumer);
    }
}
