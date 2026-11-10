using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartFridgeBackend.Controllers;
using SmartFridgeBackend.DTOs.Recipes;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.BackendTests;

public class RecipesControllerTests
{
    private readonly Mock<IRecipeGenerationService> _recipeGenerationServiceMock;
    private readonly Mock<IRecipeFavoritesService> _recipeFavoritesServiceMock;
    private readonly RecipesController _controller;

    public RecipesControllerTests()
    {
        _recipeGenerationServiceMock = new Mock<IRecipeGenerationService>();
        _recipeFavoritesServiceMock = new Mock<IRecipeFavoritesService>();

        _controller = new RecipesController(
            _recipeGenerationServiceMock.Object,
            _recipeFavoritesServiceMock.Object
        );
    }

    private void SetUserWithUid(string uid)
    {
        var claims = new List<Claim>
        {
            new Claim("uid", uid)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private void SetUserWithoutUid()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task Generate_WithValidRequestAndUid_ReturnsOk()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" },
            MaxMinutes = 30
        };

        var recipes = new List<GeneratedRecipeResponse>
        {
            new GeneratedRecipeResponse
            {
                Title = "Omelette",
                Servings = 2,
                TimeMinutes = 10,
                Ingredients = new List<GeneratedIngredientResponse>
                {
                    new GeneratedIngredientResponse
                    {
                        Name = "Eggs",
                        Amount = 2,
                        Unit = "pcs"
                    }
                },
                Steps = new List<string> { "Beat eggs", "Cook eggs", "Serve" }
            }
        };

        _recipeGenerationServiceMock
            .Setup(s => s.GenerateRecipesAsync(request))
            .ReturnsAsync(recipes);

        // Act
        var result = await _controller.Generate(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<List<GeneratedRecipeResponse>>(okResult.Value);
        Assert.Single(value);
        Assert.Equal("Omelette", value[0].Title);
    }

    [Fact]
    public async Task Generate_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs" }
        };

        // Act
        var result = await _controller.Generate(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _recipeGenerationServiceMock.Verify(s => s.GenerateRecipesAsync(It.IsAny<GenerateRecipesRequest>()), Times.Never);
    }

    [Fact]
    public async Task Generate_WithInvalidModelState_ReturnsValidationProblem()
    {
        // Arrange
        SetUserWithUid("firebase-123");
        _controller.ModelState.AddModelError("Ingredients", "Ingredients are required");

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string>()
        };

        // Act
        var result = await _controller.Generate(request);

        // Assert
        Assert.IsType<ObjectResult>(result);
        _recipeGenerationServiceMock.Verify(s => s.GenerateRecipesAsync(It.IsAny<GenerateRecipesRequest>()), Times.Never);
    }

    [Fact]
    public async Task SaveFavorite_WithValidRequestAndUid_ReturnsOk()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new SaveFavoriteRecipeRequest
        {
            Title = "Pasta",
            Servings = 2,
            TimeMinutes = 20,
            Ingredients = new List<GeneratedIngredientResponse>
            {
                new GeneratedIngredientResponse
                {
                    Name = "Pasta",
                    Amount = 200,
                    Unit = "g"
                }
            },
            Steps = new List<string> { "Boil water", "Cook pasta", "Serve" }
        };

        var saved = new FavoriteRecipeResponse
        {
            Id = "recipe-1",
            Title = "Pasta",
            Servings = 2,
            TimeMinutes = 20,
            Ingredients = request.Ingredients,
            Steps = request.Steps
        };

        _recipeFavoritesServiceMock
            .Setup(s => s.SaveFavoriteAsync("firebase-123", request))
            .ReturnsAsync(saved);

        // Act
        var result = await _controller.SaveFavorite(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<FavoriteRecipeResponse>(okResult.Value);
        Assert.Equal("recipe-1", value.Id);
        Assert.Equal("Pasta", value.Title);
    }

    [Fact]
    public async Task SaveFavorite_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        var request = new SaveFavoriteRecipeRequest
        {
            Title = "Pasta",
            Servings = 2,
            TimeMinutes = 20,
            Ingredients = new List<GeneratedIngredientResponse>
            {
                new GeneratedIngredientResponse
                {
                    Name = "Pasta",
                    Amount = 200,
                    Unit = "g"
                }
            },
            Steps = new List<string> { "Boil water", "Cook pasta", "Serve" }
        };

        // Act
        var result = await _controller.SaveFavorite(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _recipeFavoritesServiceMock.Verify(s => s.SaveFavoriteAsync(It.IsAny<string>(), It.IsAny<SaveFavoriteRecipeRequest>()), Times.Never);
    }

    [Fact]
    public async Task SaveFavorite_WithInvalidModelState_ReturnsValidationProblem()
    {
        // Arrange
        SetUserWithUid("firebase-123");
        _controller.ModelState.AddModelError("Title", "Title is required");

        var request = new SaveFavoriteRecipeRequest
        {
            Title = "",
            Servings = 2,
            TimeMinutes = 20,
            Ingredients = new List<GeneratedIngredientResponse>
            {
                new GeneratedIngredientResponse
                {
                    Name = "Pasta",
                    Amount = 200,
                    Unit = "g"
                }
            },
            Steps = new List<string> { "Boil water", "Cook pasta", "Serve" }
        };

        // Act
        var result = await _controller.SaveFavorite(request);

        // Assert
        Assert.IsType<ObjectResult>(result);
        _recipeFavoritesServiceMock.Verify(s => s.SaveFavoriteAsync(It.IsAny<string>(), It.IsAny<SaveFavoriteRecipeRequest>()), Times.Never);
    }

    [Fact]
    public async Task SaveFavorite_WhenServiceReturnsNull_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new SaveFavoriteRecipeRequest
        {
            Title = "Pasta",
            Servings = 2,
            TimeMinutes = 20,
            Ingredients = new List<GeneratedIngredientResponse>
            {
                new GeneratedIngredientResponse
                {
                    Name = "Pasta",
                    Amount = 200,
                    Unit = "g"
                }
            },
            Steps = new List<string> { "Boil water", "Cook pasta", "Serve" }
        };

        _recipeFavoritesServiceMock
            .Setup(s => s.SaveFavoriteAsync("firebase-123", request))
            .ReturnsAsync((FavoriteRecipeResponse?)null);

        // Act
        var result = await _controller.SaveFavorite(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetFavorites_WithUid_ReturnsOk()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var favorites = new List<FavoriteRecipeResponse>
        {
            new FavoriteRecipeResponse
            {
                Id = "recipe-1",
                Title = "Soup",
                Servings = 4,
                TimeMinutes = 30,
                Ingredients = new List<GeneratedIngredientResponse>
                {
                    new GeneratedIngredientResponse
                    {
                        Name = "Water",
                        Amount = 1,
                        Unit = "L"
                    }
                },
                Steps = new List<string> { "Heat water", "Serve" }
            }
        };

        _recipeFavoritesServiceMock
            .Setup(s => s.GetFavoritesAsync("firebase-123"))
            .ReturnsAsync(favorites);

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<List<FavoriteRecipeResponse>>(okResult.Value);
        Assert.Single(value);
        Assert.Equal("Soup", value[0].Title);
    }

    [Fact]
    public async Task GetFavorites_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _recipeFavoritesServiceMock.Verify(s => s.GetFavoritesAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFavorite_WithExistingRecipe_ReturnsNoContent()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        _recipeFavoritesServiceMock
            .Setup(s => s.DeleteFavoriteAsync("firebase-123", "recipe-1"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFavorite("recipe-1");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFavorite_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        // Act
        var result = await _controller.DeleteFavorite("recipe-1");

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _recipeFavoritesServiceMock.Verify(s => s.DeleteFavoriteAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFavorite_WhenRecipeNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        _recipeFavoritesServiceMock
            .Setup(s => s.DeleteFavoriteAsync("firebase-123", "missing-recipe"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFavorite("missing-recipe");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}