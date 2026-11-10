using Moq;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Repositories;
using SmartFridgeBackend.DTOs.Recipes;
using SmartFridgeBackend.Services;
using RecipeModel = SmartFridgeBackend.Database.Models.Recipe;
using UserModel = SmartFridgeBackend.Database.Models.User;

namespace SmartFridgeBackend.BackendTests;

public class RecipeFavoritesServiceTests
{
    private readonly Mock<IRepository<RecipeModel>> _recipeRepositoryMock;
    private readonly Mock<IRepository<UserModel>> _userRepositoryMock;
    private readonly RecipeFavoritesService _service;

    public RecipeFavoritesServiceTests()
    {
        _recipeRepositoryMock = new Mock<IRepository<RecipeModel>>();
        _userRepositoryMock = new Mock<IRepository<UserModel>>();

        _service = new RecipeFavoritesService(
            _recipeRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    [Fact]
    public async Task SaveFavoriteAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
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

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel>());

        // Act
        var result = await _service.SaveFavoriteAsync("unknown-uid", request);

        // Assert
        Assert.Null(result);
        _recipeRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<RecipeModel>()), Times.Never);
    }

    [Fact]
    public async Task SaveFavoriteAsync_UserExists_InsertsRecipeAndReturnsMappedResponse()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new UserModel
        {
            UserId = "user-1",
            AuthId = firebaseUid,
            Email = "test@test.com"
        };

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
                },
                new GeneratedIngredientResponse
                {
                    Name = "Salt",
                    Amount = 1,
                    Unit = "tsp"
                }
            },
            Steps = new List<string> { "Boil water", "Cook pasta", "Serve" }
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel> { user });

        _recipeRepositoryMock
            .Setup(r => r.InsertAsync(It.IsAny<RecipeModel>()))
            .Callback<RecipeModel>(recipe => recipe.RecipeId = "recipe-1")
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SaveFavoriteAsync(firebaseUid, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("recipe-1", result.Id);
        Assert.Equal("Pasta", result.Title);
        Assert.Equal(2, result.Servings);
        Assert.Equal(20, result.TimeMinutes);
        Assert.Equal(2, result.Ingredients.Count);
        Assert.Equal(3, result.Steps.Count);

        _recipeRepositoryMock.Verify(r => r.InsertAsync(It.Is<RecipeModel>(recipe =>
            recipe.UserId == "user-1" &&
            recipe.Title == "Pasta" &&
            recipe.Servings == 2 &&
            recipe.TimeMinutes == 20 &&
            recipe.Ingredients.Count == 2 &&
            recipe.Steps.Count == 3
        )), Times.Once);
    }

    [Fact]
    public async Task GetFavoritesAsync_UserDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel>());

        // Act
        var result = await _service.GetFavoritesAsync("unknown-uid");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _recipeRepositoryMock.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RecipeModel, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesAsync_UserExists_ReturnsMappedFavorites()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new UserModel
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        var recipes = new List<RecipeModel>
        {
            new RecipeModel
            {
                RecipeId = "recipe-1",
                UserId = "user-1",
                Title = "Soup",
                Servings = 4,
                TimeMinutes = 30,
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient
                    {
                        Name = "Water",
                        Amount = 1,
                        Unit = "L"
                    }
                },
                Steps = new List<string> { "Heat water", "Serve" }
            }
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel> { user });

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RecipeModel, bool>>>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _service.GetFavoritesAsync(firebaseUid);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("recipe-1", result[0].Id);
        Assert.Equal("Soup", result[0].Title);
        Assert.Equal(4, result[0].Servings);
        Assert.Equal(30, result[0].TimeMinutes);
        Assert.Single(result[0].Ingredients);
        Assert.Equal(2, result[0].Steps.Count);
    }

    [Fact]
    public async Task DeleteFavoriteAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel>());

        // Act
        var result = await _service.DeleteFavoriteAsync("unknown-uid", "recipe-1");

        // Assert
        Assert.False(result);
        _recipeRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFavoriteAsync_RecipeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new UserModel
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel> { user });

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RecipeModel, bool>>>()))
            .ReturnsAsync(new List<RecipeModel>());

        // Act
        var result = await _service.DeleteFavoriteAsync(firebaseUid, "missing-recipe");

        // Assert
        Assert.False(result);
        _recipeRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFavoriteAsync_RecipeExists_ReturnsTrueAndDeletesRecipe()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new UserModel
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        var recipe = new RecipeModel
        {
            RecipeId = "recipe-1",
            UserId = "user-1",
            Title = "Soup",
            Servings = 4,
            TimeMinutes = 30,
            Ingredients = new List<RecipeIngredient>(),
            Steps = new List<string>()
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserModel, bool>>>()))
            .ReturnsAsync(new List<UserModel> { user });

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RecipeModel, bool>>>()))
            .ReturnsAsync(new List<RecipeModel> { recipe });

        _recipeRepositoryMock
            .Setup(r => r.DeleteAsync("recipe-1"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteFavoriteAsync(firebaseUid, "recipe-1");

        // Assert
        Assert.True(result);
        _recipeRepositoryMock.Verify(r => r.DeleteAsync("recipe-1"), Times.Once);
    }
}