using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;

namespace NotificationSystem.Worker.Tests.Consumers;

public class NotificationConsumerTests
{
    private const ulong DeliveryTag = 42;

    private readonly Mock<ILogger<NotificationConsumer>> _loggerMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IChannel> _channelMock = new();
    private readonly RabbitMqSettings _settings = new()
    {
        Hostname = "localhost",
        Port = "5672",
        QueueName = "user.created",
        Username = "guest",
        Password = "guest"
    };

    private NotificationConsumer CreateSut() =>
        new(_loggerMock.Object, Options.Create(_settings), _emailServiceMock.Object);

    [Fact]
    public async Task ProcessMessageAsync_WhenMessageIsValid_SendsEmailAndAcks()
    {
        var userEvent = new UserCreatedEvent(Guid.NewGuid(), "Jane Doe", "jane@example.com");
        var json = JsonSerializer.Serialize(userEvent);
        var sut = CreateSut();

        await sut.ProcessMessageAsync(json, _channelMock.Object, DeliveryTag);

        _emailServiceMock.Verify(
            s => s.SendEmailAsync(It.Is<Email>(e =>
                e.To == userEvent.Email &&
                e.Subject == "Test Subject" &&
                e.Body == "Test Body")),
            Times.Once);
        _channelMock.Verify(
            c => c.BasicAckAsync(DeliveryTag, false, It.IsAny<CancellationToken>()),
            Times.Once);
        _channelMock.Verify(
            c => c.BasicRejectAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenEmailServiceThrows_RejectsWithoutRequeue()
    {
        var userEvent = new UserCreatedEvent(Guid.NewGuid(), "Jane Doe", "jane@example.com");
        var json = JsonSerializer.Serialize(userEvent);
        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Email>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
        var sut = CreateSut();

        await sut.ProcessMessageAsync(json, _channelMock.Object, DeliveryTag);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<Email>()), Times.Once);
        _channelMock.Verify(
            c => c.BasicRejectAsync(DeliveryTag, false, It.IsAny<CancellationToken>()),
            Times.Once);
        _channelMock.Verify(
            c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenJsonIsInvalid_RejectsWithoutRequeue()
    {
        const string invalidJson = "{ not valid json";
        var sut = CreateSut();

        await sut.ProcessMessageAsync(invalidJson, _channelMock.Object, DeliveryTag);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<Email>()), Times.Never);
        _channelMock.Verify(
            c => c.BasicRejectAsync(DeliveryTag, false, It.IsAny<CancellationToken>()),
            Times.Once);
        _channelMock.Verify(
            c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenEventPayloadIsNull_RejectsWithoutRequeue()
    {
        const string nullPayloadJson = "null";
        var sut = CreateSut();

        await sut.ProcessMessageAsync(nullPayloadJson, _channelMock.Object, DeliveryTag);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<Email>()), Times.Never);
        _channelMock.Verify(
            c => c.BasicRejectAsync(DeliveryTag, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
