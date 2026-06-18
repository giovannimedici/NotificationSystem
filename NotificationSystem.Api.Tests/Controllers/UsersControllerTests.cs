using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationSystem.Api.Contracts.Requests;

namespace NotificationSystem.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();

    private UsersController CreateSut() =>
        new(_userServiceMock.Object);

    [Fact]
    public async Task Create_CallsUserServiceWithDto()
    {
        var request = new CreateUserRequest { Name = "João", Email = "joao@email.com" };
        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(true);
        var sut = CreateSut();

        await sut.Create(request);

        _userServiceMock.Verify(
            s => s.CreateUserAsync(It.Is<UserDto>(d => d.Name == request.Name && d.Email == request.Email)),
            Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsOkWithExpectedMessage()
    {
        var request = new CreateUserRequest { Name = "Ana", Email = "ana@email.com" };
        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User created and message sent.", okResult.Value);
    }

    [Fact]
    public async Task Create_WhenUserServiceReturnsFalse_ReturnsBadRequest()
    {
        var request = new CreateUserRequest { Name = "Pedro", Email = "pedro@email.com" };
        _userServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(false);
        var sut = CreateSut();

        var result = await sut.Create(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
