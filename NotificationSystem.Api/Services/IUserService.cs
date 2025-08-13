using NotificationSystem.Shared.Models;

public interface IUserService
{
    Task CreateUserAsync(UserDto dto);
}
