using SmartFridgeBackend.DTOs.Recipes;

namespace SmartFridgeBackend.Services;

public interface IRecipeFavoritesService
{
    Task<FavoriteRecipeResponse?> SaveFavoriteAsync(string firebaseUid, SaveFavoriteRecipeRequest request);
    Task<List<FavoriteRecipeResponse>> GetFavoritesAsync(string firebaseUid);
    Task<bool> DeleteFavoriteAsync(string firebaseUid, string recipeId);
}