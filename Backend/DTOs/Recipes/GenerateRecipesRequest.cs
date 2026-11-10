using System.ComponentModel.DataAnnotations;

namespace SmartFridgeBackend.DTOs.Recipes;

public class GenerateRecipesRequest
{
    [Required]
    [MinLength(1)]
    public List<string> Ingredients { get; set; } = new();

    public string? Preferences { get; set; }

    [Range(1, 300)]
    public int? MaxMinutes { get; set; }
}