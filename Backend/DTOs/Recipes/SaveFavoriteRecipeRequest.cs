using System.ComponentModel.DataAnnotations;

namespace SmartFridgeBackend.DTOs.Recipes;

public class SaveFavoriteRecipeRequest
{
    [Required]
    public string Title { get; set; } = "";

    [Range(1, 20)]
    public int Servings { get; set; }

    [Range(1, 300)]
    public int TimeMinutes { get; set; }

    [Required]
    [MinLength(1)]
    public List<GeneratedIngredientResponse> Ingredients { get; set; } = new();

    [Required]
    [MinLength(1)]
    public List<string> Steps { get; set; } = new();
}