using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFridgeBackend.DTOs.Recipes;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize(Policy = "EmailVerified")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeGenerationService _recipeGenerationService;
    private readonly IRecipeFavoritesService _recipeFavoritesService;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(IRecipeGenerationService recipeGenerationService, IRecipeFavoritesService recipeFavoritesService, ILogger<RecipesController> logger)
    {
        _recipeGenerationService = recipeGenerationService;
        _recipeFavoritesService = recipeFavoritesService;
        _logger = logger;
    }

    // helper
    private string? GetFirebaseUid()
    {
        return User.FindFirst("uid")?.Value;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRecipesRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request to generate recipes");
            return Unauthorized();
        }

        try
        {
            var recipes = await _recipeGenerationService.GenerateRecipesAsync(request);
            _logger.LogInformation("Recipes generated for user {FirebaseUid}", uid);
            return Ok(recipes);
        }
        catch (ApplicationException ex) // error message if recipe generations fails.
        {
            _logger.LogWarning(ex, "Could not generate recipes for user {FirebaseUid}", uid);
            return StatusCode(500, new { error = ex.Message});
        }
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> SaveFavorite([FromBody] SaveFavoriteRecipeRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request to save recipe to favorites");
            return Unauthorized();
        }

        var saved = await _recipeFavoritesService.SaveFavoriteAsync(uid, request);
        if (saved == null)
        {
            _logger.LogWarning("Unauthorized request to save recipe to favorites");
            return Unauthorized();
        }
        
        _logger.LogInformation("Recipe saved for user {FirebaseUid}", uid);
        return Ok(saved);
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request for favorites");
            return Unauthorized();
        }

        var favorites = await _recipeFavoritesService.GetFavoritesAsync(uid);
        _logger.LogInformation("Successful request for favorites for user {FirebaseUid}", uid);
        return Ok(favorites);
    }

    [HttpDelete("favorites/{recipeId}")]
    public async Task<IActionResult> DeleteFavorite(string recipeId)
    {
        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request for delete favorite recipe");
            return Unauthorized();
        }

        var success = await _recipeFavoritesService.DeleteFavoriteAsync(uid, recipeId);
        if (!success)
        {
            _logger.LogWarning("Recipe {recipeId} not found for user {FirebaseUid}", recipeId, uid);
            return NotFound(new { error = "Opskriften blev ikke fundet" });
        }

        _logger.LogInformation("Favorite recipe {recipeId} successfully deleted for user {FirebaseUid}", recipeId, uid);
        return NoContent();
    }
}