using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationSystem.Api.Messaging;
using RabbitMQ.Client;

public class RabbitMqMessageBus : IRabbitMqMessageBus, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
 
    private const string DlxName = "system.dlx";
    private const string DlqName = "expired.queue";
    private const string DlqRoutingKey = "message.expired";
    private const string MainExchange = "system.exchange";
    private const string MainRoutingKey = "message.main";
    private const int MessageTtlMs = 15_000;

    public RabbitMqMessageBus(IOptions<RabbitMqSettings> options, ILogger<RabbitMqMessageBus> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    private async Task<IChannel> GetChannelAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_channel is { IsOpen: true })
                return _channel;
 
            _logger.LogInformation("Opening RabbitMQ connection to {Host}:{Port}", _settings.Hostname, _settings.Port);
 
            var factory = new ConnectionFactory
            {
                HostName = _settings.Hostname,
                Port = int.Parse(_settings.Port),
                UserName = _settings.Username,
                Password = _settings.Password
            };
 
            var endpoints = new List<AmqpTcpEndpoint>
            {
                new(_settings.Hostname, int.Parse(_settings.Port)),
                new("rabbitmq", int.Parse(_settings.Port))
            };
 
            _connection = await factory.CreateConnectionAsync(endpoints);
            _channel = await _connection.CreateChannelAsync();
 
            await DeclareTopologyAsync(_channel);
 
            return _channel;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(exchange: DlxName, type: ExchangeType.Direct, durable: true);
        await channel.QueueDeclareAsync(queue: DlqName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queue: DlqName, exchange: DlxName, routingKey: DlqRoutingKey);
 
        await channel.ExchangeDeclareAsync(exchange: MainExchange, type: ExchangeType.Direct, durable: true);
 
        var queueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", DlxName },
            { "x-dead-letter-routing-key", DlqRoutingKey },
            { "x-message-ttl", MessageTtlMs }
        };
 
        await channel.QueueDeclareAsync(queue: "mainQueue", durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
        await channel.QueueBindAsync(queue: "mainQueue", exchange: MainExchange, routingKey: MainRoutingKey);
    }

    public async Task Publish<T>(T message, string queueName)
    {
        var channel = await GetChannelAsync();
 
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
 
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };
 
        await channel.BasicPublishAsync(MainExchange, MainRoutingKey, mandatory: true, basicProperties: props, body: body);
        _logger.LogInformation("Message published to queue {QueueName}: {MessageType}", queueName, typeof(T).Name);
    }
 
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();
 
        if (_connection is not null)
            await _connection.CloseAsync();
    }
   
}
