using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Contracts.Requests;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var dto = new UserDto(request.Name, request.Email);

        if (!await _userService.CreateUserAsync(dto))
            return BadRequest("An error occurred while creating the user or sending the message.");

        return Ok("User created and message sent.");
    }
}
