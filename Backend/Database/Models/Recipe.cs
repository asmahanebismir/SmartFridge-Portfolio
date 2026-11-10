using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartFridgeBackend.Database.Models;

// Repræsenterer en opskrift
public class Recipe
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? RecipeId { get; set; }  // Unik ID for opskriften

    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; } // Unique id for user. 

    public string? Title { get; set; }     // Opskriftens navn

    public int Servings { get; set; } // Antal serveringer

    public int TimeMinutes { get; set; } // Tid

    public List<RecipeIngredient> Ingredients { get; set; } = new();  // Liste af ingredienser

    public List<string> Steps { get; set; } = new(); // Steps fremfor instructions. For senere at kunne markere hvert trin. Ikke sikker endnu.

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RecipeIngredient
{
    public string Name { get; set; } = "";
    public double Amount { get; set; }
    public string Unit { get; set; } = "";
}