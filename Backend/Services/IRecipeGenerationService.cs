using SmartFridgeBackend.DTOs.Recipes;

namespace SmartFridgeBackend.Services;

public interface IRecipeGenerationService
{
    Task<List<GeneratedRecipeResponse>> GenerateRecipesAsync(GenerateRecipesRequest request);
}