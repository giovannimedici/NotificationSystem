using System.Text;
using System.Text.Json;
using NotificationSystem.Shared.Models;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task CreateUserAsync(UserDto dto)
    {
        var user = new User { Name = dto.Name, Email = dto.Email };
        await _userRepository.AddUserAsync(user);


    }
}
