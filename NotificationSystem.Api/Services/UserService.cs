using NotificationSystem.Api.Messaging;
using NotificationSystem.Shared.Models;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IRabbitMqMessageBus _rabbitMqMessageBus;

    public UserService(IUserRepository userRepository, IRabbitMqMessageBus rabbitMqMessageBus, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _rabbitMqMessageBus = rabbitMqMessageBus;
        _logger = logger;
    }

    public async Task<bool> CreateUserAsync(UserDto dto)
    {
        try
        {
            var user = new User { Name = dto.Name, Email = dto.Email };
            await _userRepository.AddUserAsync(user);
            _logger.LogInformation($"User created with email: {dto.Email}");

            await _rabbitMqMessageBus.Publish(new UserCreatedEvent(Guid.NewGuid(), dto.Name, dto.Email), "mainQueue");
            _logger.LogInformation($"Published UserCreatedEvent for email: {dto.Email}");

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError($"Failed to create user or publish message for email: {dto.Email}: {ex.Message}");
            return false;
        }
    }
}
