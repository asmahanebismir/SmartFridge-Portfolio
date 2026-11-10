using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Repositories;
using SmartFridgeBackend.DTOs.Recipes;
using RecipeModel = SmartFridgeBackend.Database.Models.Recipe;
using UserModel = SmartFridgeBackend.Database.Models.User;

namespace SmartFridgeBackend.Services;

public class RecipeFavoritesService : IRecipeFavoritesService
{
    private readonly IRepository<RecipeModel> _recipeRepository;
    private readonly IRepository<UserModel> _userRepository;

    public RecipeFavoritesService(
        IRepository<RecipeModel> recipeRepository,
        IRepository<UserModel> userRepository)
    {
        _recipeRepository = recipeRepository;
        _userRepository = userRepository;
    }

    private async Task<UserModel?> GetUserByFirebaseUid(string firebaseUid)
    {
        var users = await _userRepository.FindAsync(u => u.AuthId == firebaseUid);
        return users.FirstOrDefault();
    }

    public async Task<FavoriteRecipeResponse?> SaveFavoriteAsync(string firebaseUid, SaveFavoriteRecipeRequest request)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return null;

        var recipe = new RecipeModel
        {
            UserId = user.UserId,
            Title = request.Title,
            Servings = request.Servings,
            TimeMinutes = request.TimeMinutes,
            Ingredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                Name = i.Name,
                Amount = i.Amount,
                Unit = i.Unit
            }).ToList(),
            Steps = request.Steps,
            CreatedAt = DateTime.UtcNow
        };

        await _recipeRepository.InsertAsync(recipe);

        return new FavoriteRecipeResponse
        {
            Id = recipe.RecipeId!,
            Title = recipe.Title,
            Servings = recipe.Servings,
            TimeMinutes = recipe.TimeMinutes,
            Ingredients = recipe.Ingredients.Select(i => new GeneratedIngredientResponse
            {
                Name = i.Name,
                Amount = i.Amount,
                Unit = i.Unit
            }).ToList(),
            Steps = recipe.Steps
        };
    }

    public async Task<List<FavoriteRecipeResponse>> GetFavoritesAsync(string firebaseUid)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return new List<FavoriteRecipeResponse>();

        var recipes = await _recipeRepository.FindAsync(r => r.UserId == user.UserId);

        return recipes.Select(r => new FavoriteRecipeResponse
        {
            Id = r.RecipeId!,
            Title = r.Title,
            Servings = r.Servings,
            TimeMinutes = r.TimeMinutes,
            Ingredients = r.Ingredients.Select(i => new GeneratedIngredientResponse
            {
                Name = i.Name,
                Amount = i.Amount,
                Unit = i.Unit
            }).ToList(),
            Steps = r.Steps
        }).ToList();
    }

    public async Task<bool> DeleteFavoriteAsync(string firebaseUid, string recipeId)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return false;

        var recipes = await _recipeRepository.FindAsync(r => r.RecipeId == recipeId && r.UserId == user.UserId);
        var recipe = recipes.FirstOrDefault();

        if (recipe == null) return false;

        await _recipeRepository.DeleteAsync(recipeId);
        return true;
    }
}