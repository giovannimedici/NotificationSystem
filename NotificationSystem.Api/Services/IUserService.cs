using NotificationSystem.Shared.Models;

public interface IUserService
{
    Task<bool> CreateUserAsync(UserDto dto);
}
