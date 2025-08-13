using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationSystem.Shared.Models;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _users = database.GetCollection<User>(settings.Value.CollectionName);
    }

    public async Task AddUserAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }
}