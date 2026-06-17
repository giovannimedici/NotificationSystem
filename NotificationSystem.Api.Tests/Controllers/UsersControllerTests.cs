using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationSystem.Api.Messaging;

namespace NotificationSystem.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IRabbitMqMessageBus> _messageBusMock = new();

    private UsersController CreateSut() =>
        new(_userServiceMock.Object, _messageBusMock.Object);

    [Fact]
    public async Task Create_CallsUserServiceWithDto()
    {
        var dto = new UserDto("João", "joao@email.com");
        var sut = CreateSut();

        await sut.Create(dto);

        _userServiceMock.Verify(s => s.CreateUserAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Create_PublishesUserCreatedEventOnCorrectQueue()
    {
        var dto = new UserDto("Maria", "maria@email.com");
        var sut = CreateSut();

        await sut.Create(dto);

        _messageBusMock.Verify(
            bus => bus.Publish(
                It.Is<UserCreatedEvent>(e =>
                    e.Name == dto.Name &&
                    e.Email == dto.Email &&
                    e.Id != Guid.Empty),
                "mainQueue"),
            Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsOkWithExpectedMessage()
    {
        var dto = new UserDto("Ana", "ana@email.com");
        var sut = CreateSut();

        var result = await sut.Create(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User created and message sent.", okResult.Value);
    }

    [Fact]
    public async Task Create_WhenUserServiceThrows_DoesNotPublishMessage()
    {
        var dto = new UserDto("Pedro", "pedro@email.com");
        _userServiceMock
            .Setup(s => s.CreateUserAsync(dto))
            .ThrowsAsync(new InvalidOperationException("database failure"));
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Create(dto));

        _messageBusMock.Verify(
            bus => bus.Publish(It.IsAny<UserCreatedEvent>(), It.IsAny<string>()),
            Times.Never);
    }
}
