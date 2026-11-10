namespace SmartFridgeBackend.DTOs.Recipes;

public class GeneratedRecipeResponse
{
    public string Title { get; set; } = "";
    public int Servings { get; set; }
    public int TimeMinutes { get; set; }
    public List<GeneratedIngredientResponse> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public int MissingIngredients { get; set; }
}