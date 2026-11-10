using Moq;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Repositories;
using SmartFridgeBackend.DTOs.Groceries;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.BackendTests;

public class GroceriesServiceTests
{
    private readonly Mock<IRepository<Item>> _itemRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly GroceriesService _service;

    public GroceriesServiceTests()
    {
        _itemRepositoryMock = new Mock<IRepository<Item>>();
        _userRepositoryMock = new Mock<IRepository<User>>();

        _service = new GroceriesService(
            _itemRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetMineAsync_UserExists_ReturnsMappedGroceries()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid,
            Email = "test@test.com"
        };

        var items = new List<Item>
        {
            new Item
            {
                ItemId = "item-1",
                UserId = "user-1",
                Name = "Milk",
                Amount = 2,
                Barcode = "12345"
            },
            new Item
            {
                ItemId = "item-2",
                UserId = "user-1",
                Name = "Eggs",
                Amount = 6,
                Barcode = null
            }
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Item, bool>>>()))
            .ReturnsAsync(items);

        // Act
        var result = await _service.GetMineAsync(firebaseUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal("item-1", result[0].Id);
        Assert.Equal("Milk", result[0].Name);
        Assert.Equal(2, result[0].Amount);
        Assert.Equal("12345", result[0].Barcode);

        Assert.Equal("item-2", result[1].Id);
        Assert.Equal("Eggs", result[1].Name);
        Assert.Equal(6, result[1].Amount);
        Assert.Null(result[1].Barcode);
    }

    [Fact]
    public async Task GetMineAsync_UserDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.GetMineAsync("unknown-uid");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_UserExists_InsertsItemAndReturnsResponse()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        var request = new CreateGroceriesRequest
        {
            Name = "Butter",
            Amount = 1,
            Barcode = "99999"
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.InsertAsync(It.IsAny<Item>()))
            .Callback<Item>(item => item.ItemId = "generated-item-id")
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddAsync(firebaseUid, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("generated-item-id", result.Id);
        Assert.Equal("Butter", result.Name);
        Assert.Equal(1, result.Amount);
        Assert.Equal("99999", result.Barcode);

        _itemRepositoryMock.Verify(r => r.InsertAsync(It.Is<Item>(i =>
            i.UserId == "user-1" &&
            i.Name == "Butter" &&
            i.Amount == 1 &&
            i.Barcode == "99999"
        )), Times.Once);
    }

    [Fact]
    public async Task AddAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var request = new CreateGroceriesRequest
        {
            Name = "Butter",
            Amount = 1
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.AddAsync("unknown-uid", request);

        // Assert
        Assert.Null(result);
        _itemRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ItemBelongsToUser_ReturnsTrue()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        var item = new Item
        {
            ItemId = "item-1",
            UserId = "user-1",
            Name = "Milk",
            Amount = 2
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Item, bool>>>()))
            .ReturnsAsync(new List<Item> { item });

        _itemRepositoryMock
            .Setup(r => r.DeleteAsync("item-1"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(firebaseUid, "item-1");

        // Assert
        Assert.True(result);
        _itemRepositoryMock.Verify(r => r.DeleteAsync("item-1"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Item, bool>>>()))
            .ReturnsAsync(new List<Item>());

        // Act
        var result = await _service.DeleteAsync(firebaseUid, "missing-item");

        // Assert
        Assert.False(result);
        _itemRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAmountAsync_ItemBelongsToUser_UpdatesAndReturnsTrue()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        var item = new Item
        {
            ItemId = "item-1",
            UserId = "user-1",
            Name = "Milk",
            Amount = 2
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Item, bool>>>()))
            .ReturnsAsync(new List<Item> { item });

        _itemRepositoryMock
            .Setup(r => r.ReplaceAsync("item-1", It.IsAny<Item>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAmountAsync(firebaseUid, "item-1", 5);

        // Assert
        Assert.True(result);
        Assert.Equal(5, item.Amount);

        _itemRepositoryMock.Verify(r => r.ReplaceAsync("item-1", It.Is<Item>(i =>
            i.ItemId == "item-1" &&
            i.Amount == 5
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAmountAsync_ItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var firebaseUid = "firebase-123";
        var user = new User
        {
            UserId = "user-1",
            AuthId = firebaseUid
        };

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _itemRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Item, bool>>>()))
            .ReturnsAsync(new List<Item>());

        // Act
        var result = await _service.UpdateAmountAsync(firebaseUid, "missing-item", 5);

        // Assert
        Assert.False(result);
        _itemRepositoryMock.Verify(r => r.ReplaceAsync(It.IsAny<string>(), It.IsAny<Item>()), Times.Never);
    }
}