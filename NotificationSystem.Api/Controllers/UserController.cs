using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Messaging;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRabbitMqMessageBus _messageBus;

    public UsersController(IUserService userService, IRabbitMqMessageBus messageBus)
    {
        _userService = userService;
        _messageBus = messageBus;
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserDto dto)
    {
        await _userService.CreateUserAsync(dto);

        await _messageBus.Publish(new UserCreatedEvent(Guid.NewGuid(), dto.Name, dto.Email), "new user.created");
        return Ok("Usuário criado e mensagem enviada.");
    }
}
