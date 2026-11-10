using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartFridgeBackend.Controllers;
using SmartFridgeBackend.DTOs.Groceries;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.BackendTests;

public class GroceriesControllerTests
{
    private readonly Mock<IGroceriesService> _groceriesServiceMock;
    private readonly GroceriesController _controller;

    public GroceriesControllerTests()
    {
        _groceriesServiceMock = new Mock<IGroceriesService>();
        _controller = new GroceriesController(_groceriesServiceMock.Object);
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
    public async Task GetMine_WithUid_ReturnsOkWithGroceries()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var groceries = new List<GroceriesResponse>
        {
            new GroceriesResponse
            {
                Id = "item-1",
                Name = "Milk",
                Amount = 2,
                Barcode = "12345"
            }
        };

        _groceriesServiceMock
            .Setup(s => s.GetMineAsync("firebase-123"))
            .ReturnsAsync(groceries);

        // Act
        var result = await _controller.GetMine();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<List<GroceriesResponse>>(okResult.Value);
        Assert.Single(value);
        Assert.Equal("Milk", value[0].Name);
    }

    [Fact]
    public async Task GetMine_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        // Act
        var result = await _controller.GetMine();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _groceriesServiceMock.Verify(s => s.GetMineAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Add_WithValidRequestAndUid_ReturnsOk()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new CreateGroceriesRequest
        {
            Name = "Butter",
            Amount = 1,
            Barcode = "99999"
        };

        var created = new GroceriesResponse
        {
            Id = "item-1",
            Name = "Butter",
            Amount = 1,
            Barcode = "99999"
        };

        _groceriesServiceMock
            .Setup(s => s.AddAsync("firebase-123", request))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Add(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<GroceriesResponse>(okResult.Value);
        Assert.Equal("Butter", value.Name);
        Assert.Equal(1, value.Amount);
    }

    [Fact]
    public async Task Add_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        var request = new CreateGroceriesRequest
        {
            Name = "Butter",
            Amount = 1
        };

        // Act
        var result = await _controller.Add(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _groceriesServiceMock.Verify(s => s.AddAsync(It.IsAny<string>(), It.IsAny<CreateGroceriesRequest>()), Times.Never);
    }

    [Fact]
    public async Task Add_WithInvalidModelState_ReturnsValidationProblem()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        _controller.ModelState.AddModelError("Name", "The Name field is required.");

        var request = new CreateGroceriesRequest
        {
            Name = "",
            Amount = 0
        };

        // Act
        var result = await _controller.Add(request);

        // Assert
        Assert.IsType<ObjectResult>(result);
        _groceriesServiceMock.Verify(s => s.AddAsync(It.IsAny<string>(), It.IsAny<CreateGroceriesRequest>()), Times.Never);
    }

    [Fact]
    public async Task Add_WhenServiceReturnsNull_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new CreateGroceriesRequest
        {
            Name = "Butter",
            Amount = 1
        };

        _groceriesServiceMock
            .Setup(s => s.AddAsync("firebase-123", request))
            .ReturnsAsync((GroceriesResponse?)null);

        // Act
        var result = await _controller.Add(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_WithUidAndExistingItem_ReturnsNoContent()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        _groceriesServiceMock
            .Setup(s => s.DeleteAsync("firebase-123", "item-1"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("item-1");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        // Act
        var result = await _controller.Delete("item-1");

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _groceriesServiceMock.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenItemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        _groceriesServiceMock
            .Setup(s => s.DeleteAsync("firebase-123", "missing-item"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("missing-item");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateAmount_WithValidRequestAndUid_ReturnsNoContent()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new UpdateGroceriesAmountRequest
        {
            Amount = 5
        };

        _groceriesServiceMock
            .Setup(s => s.UpdateAmountAsync("firebase-123", "item-1", 5))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateAmount("item-1", request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAmount_WithoutUid_ReturnsUnauthorized()
    {
        // Arrange
        SetUserWithoutUid();

        var request = new UpdateGroceriesAmountRequest
        {
            Amount = 5
        };

        // Act
        var result = await _controller.UpdateAmount("item-1", request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _groceriesServiceMock.Verify(s => s.UpdateAmountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAmount_WithInvalidModelState_ReturnsValidationProblem()
    {
        // Arrange
        SetUserWithUid("firebase-123");
        _controller.ModelState.AddModelError("Amount", "Invalid amount");

        var request = new UpdateGroceriesAmountRequest
        {
            Amount = -1
        };

        // Act
        var result = await _controller.UpdateAmount("item-1", request);

        // Assert
        Assert.IsType<ObjectResult>(result);
        _groceriesServiceMock.Verify(s => s.UpdateAmountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAmount_WhenItemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserWithUid("firebase-123");

        var request = new UpdateGroceriesAmountRequest
        {
            Amount = 5
        };

        _groceriesServiceMock
            .Setup(s => s.UpdateAmountAsync("firebase-123", "missing-item", 5))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateAmount("missing-item", request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}