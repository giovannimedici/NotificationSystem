using NotificationSystem.Shared.Models;

public interface IUserRepository
{
    Task AddUserAsync(User user);
}