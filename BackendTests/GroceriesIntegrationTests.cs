using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Models;
using Xunit;

namespace BackendTests;

public class GroceriesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GroceriesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMine_Returns_Only_Current_Users_Items()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var users = db.GetCollection<User>("Users");
        var items = db.GetCollection<Item>("Items");

        var user = new User
        {
            UserId = "507f1f77bcf86cd799439011",
            AuthId = "test-firebase-uid",
            Email = "test@test.dk"
        };

        var otherUser = new User
        {
            UserId = "507f1f77bcf86cd799439012",
            AuthId = "other-firebase-uid",
            Email = "other@test.dk"
        };

        await users.InsertManyAsync(new[] { user, otherUser });

        await items.InsertManyAsync(new[]
        {
            new Item
            {
                UserId = user.UserId,
                Name = "Mælk",
                Amount = 2,
                Barcode = "123"
            },
            new Item
            {
                UserId = otherUser.UserId,
                Name = "Tomat",
                Amount = 1,
                Barcode = "456"
            }
        });

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/groceries");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<GroceriesDtoForTest>>();

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Mælk", result[0].Name);
    }

    [Fact]
    public async Task Add_Creates_Item_In_Database()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var users = db.GetCollection<User>("Users");
        var items = db.GetCollection<Item>("Items");

        var user = new User
        {
            UserId = "507f1f77bcf86cd799439011",
            AuthId = "test-firebase-uid",
            Email = "test@test.dk"
        };

        await users.InsertOneAsync(user);

        var client = _factory.CreateClient();

        var payload = new
        {
            name = "Letmælk",
            amount = 1,
            barcode = "5711953060748",
            imageUrl = "https://example.com/milk.jpg"
        };

        var response = await client.PostAsJsonAsync("/api/groceries/add", payload);

        response.EnsureSuccessStatusCode();

        var createdItem = await items.Find(i => i.Name == "Letmælk").FirstOrDefaultAsync();

        Assert.NotNull(createdItem);
        Assert.Equal(1, createdItem.Amount);
        Assert.Equal("5711953060748", createdItem.Barcode);
        Assert.Equal("https://example.com/milk.jpg", createdItem.ImageUrl);
    }

    [Fact]
    public async Task Delete_Removes_Item()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var users = db.GetCollection<User>("Users");
        var items = db.GetCollection<Item>("Items");

        var user = new User
        {
            UserId = "507f1f77bcf86cd799439011",
            AuthId = "test-firebase-uid",
            Email = "test@test.dk"
        };

        await users.InsertOneAsync(user);

        var item = new Item
        {
            UserId = user.UserId,
            Name = "Pepsi Max",
            Amount = 1,
            Barcode = "5741000158432"
        };

        await items.InsertOneAsync(item);

        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/groceries/{item.ItemId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deleted = await items.Find(i => i.ItemId == item.ItemId).FirstOrDefaultAsync();
        Assert.Null(deleted);
    }

    [Fact]
    public async Task UpdateAmount_Changes_Amount()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var users = db.GetCollection<User>("Users");
        var items = db.GetCollection<Item>("Items");

        var user = new User
        {
            UserId = "507f1f77bcf86cd799439011",
            AuthId = "test-firebase-uid",
            Email = "test@test.dk"
        };

        await users.InsertOneAsync(user);

        var item = new Item
        {
            UserId = user.UserId,
            Name = "Tomat",
            Amount = 1,
            Barcode = "5703333333333"
        };

        await items.InsertOneAsync(item);

        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/groceries/{item.ItemId}/amount",
            new { amount = 5 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await items.Find(i => i.ItemId == item.ItemId).FirstOrDefaultAsync();

        Assert.NotNull(updated);
        Assert.Equal(5, updated!.Amount);
    }

    private class GroceriesDtoForTest
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Amount { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
    }
}