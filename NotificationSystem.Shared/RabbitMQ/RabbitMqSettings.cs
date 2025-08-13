public class RabbitMqSettings
{
    public string Hostname { get; set; } = "rabbitmq";
    public string QueueName { get; set; } = "user.created";
    public string Port { get; set; } = "5672"; // Default RabbitMQ port
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
