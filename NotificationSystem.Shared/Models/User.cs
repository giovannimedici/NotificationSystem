using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationSystem.Shared.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
}

