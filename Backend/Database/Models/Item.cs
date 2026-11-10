using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartFridgeBackend.Database.Models;

// Repræsenterer en vare i køleskabet
public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ItemId { get; set; }  // Unik ID for varen

    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }  // Reference til Bruger.UserId

    public string? Name { get; set; }    // Navn på varen (fx "Mælk")

    public string? Barcode { get; set; } // Stregkode 

    public int Amount { get; set; } // Antal af vare

    public string? ImageUrl { get; set; } // Billede af varen
}
