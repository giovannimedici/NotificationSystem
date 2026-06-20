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
    private readonly IEmailService _emailService;

    public NotificationConsumer(ILogger<NotificationConsumer> logger, IOptions<RabbitMqSettings> options, IEmailService emailService)
    {
        _logger = logger;
        _settings = options.Value;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Connecting to RabbitMQ at {_settings.Hostname}:{_settings.Port}");
        await Task.Delay(30000);

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

        _logger.LogInformation("Succesfully connected to rabbitmq...");
        var queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "system.dlx" },
            { "x-dead-letter-routing-key", "message.expired" },
            { "x-message-ttl", 15000 }
        };
        await channel.QueueDeclareAsync(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);

        _logger.LogInformation($"Waiting for messages in queue: {_settings.QueueName}");
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            await ProcessMessageAsync(json, channel, ea.DeliveryTag, stoppingToken);
        };

        await channel.BasicConsumeAsync(queue: _settings.QueueName, autoAck: false, consumer: consumer);
    }

    internal async Task ProcessMessageAsync(
        string json,
        IChannel channel,
        ulong deliveryTag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<UserCreatedEvent>(json)
                ?? throw new JsonException("Deserialized event is null.");

            _logger.LogInformation($"Received event: {json}");
            _logger.LogInformation($"Sending email to {@event.Email}");

            await _emailService.SendEmailAsync(new Email(@event.Email, "Test Subject", "Test Body"));
            await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken);

            _logger.LogInformation($"Successfully processed message: {json}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing message: {json}");
            await channel.BasicRejectAsync(deliveryTag, requeue: false, cancellationToken);
        }
    }
}
