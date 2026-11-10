using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartFridgeBackend.Database.Models;

// Repræsenterer en bruger i systemet
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }  // MongoDB genereret ID
    [BsonElement("email")]
    public string? Email { get; set; }   // Brugerens e-mail
    [BsonElement("firebaseUid")]
    public string? AuthId { get; set; }  // Firebase UID
    [BsonElement("oprettetDen")]
    public DateTime? CreatedAt { get; set; }
}
