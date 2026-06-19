using Microsoft.Extensions.Logging;
using Moq;
using NotificationSystem.Api.Messaging;
using NotificationSystem.Shared.Models;

namespace NotificationSystem.Api.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRabbitMqMessageBus> _rabbitMqMessageBusMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();

    private UserService CreateSut() =>
        new(_userRepositoryMock.Object, _rabbitMqMessageBusMock.Object, _loggerMock.Object);

    [Fact]
    public async Task CreateUserAsync_WhenSuccessful_ReturnsTrue()
    {
        var dto = new UserDto("João", "joao@email.com");
        var sut = CreateSut();

        var result = await sut.CreateUserAsync(dto);

        Assert.True(result);
    }

    [Fact]
    public async Task CreateUserAsync_WhenSuccessful_AddsUserToRepository()
    {
        var dto = new UserDto("João", "joao@email.com");
        var sut = CreateSut();

        await sut.CreateUserAsync(dto);

        _userRepositoryMock.Verify(
            r => r.AddUserAsync(It.Is<User>(u => u.Name == dto.Name && u.Email == dto.Email)),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenSuccessful_PublishesUserCreatedEvent()
    {
        var dto = new UserDto("Ana", "ana@email.com");
        var sut = CreateSut();

        await sut.CreateUserAsync(dto);

        _rabbitMqMessageBusMock.Verify(
            b => b.Publish(
                It.Is<UserCreatedEvent>(e => e.Name == dto.Name && e.Email == dto.Email),
                "mainQueue"),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRepositoryThrows_ReturnsFalse()
    {
        var dto = new UserDto("Pedro", "pedro@email.com");
        _userRepositoryMock
            .Setup(r => r.AddUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));
        var sut = CreateSut();

        var result = await sut.CreateUserAsync(dto);

        Assert.False(result);
        _rabbitMqMessageBusMock.Verify(
            b => b.Publish(It.IsAny<UserCreatedEvent>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenMessageBusThrows_ReturnsFalse()
    {
        var dto = new UserDto("Maria", "maria@email.com");
        _rabbitMqMessageBusMock
            .Setup(b => b.Publish(It.IsAny<UserCreatedEvent>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Queue unavailable"));
        var sut = CreateSut();

        var result = await sut.CreateUserAsync(dto);

        Assert.False(result);
        _userRepositoryMock.Verify(
            r => r.AddUserAsync(It.Is<User>(u => u.Name == dto.Name && u.Email == dto.Email)),
            Times.Once);
    }
}
