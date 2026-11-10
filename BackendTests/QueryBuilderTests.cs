using Moq;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Query;

namespace SmartFridgeBackend.BackendTests;

public class QueryBuilderTests
{
    // Test at QueryBuilder<T> kan instantieres korrekt
    [Fact]
    public void QueryBuilder_CanBeInstantiated()
    {
        // Arrange
        var mockCollection = new Mock<IMongoCollection<Item>>();

        // Act
        var builder = new QueryBuilder<Item>(mockCollection.Object);

        // Assert
        Assert.NotNull(builder);
    }

    // Test at Where() returnerer samme builder (method chaining)
    [Fact]
    public void QueryBuilder_Where_ReturnsItself_ForChaining()
    {
        var mockCollection = new Mock<IMongoCollection<Item>>();
        var builder = new QueryBuilder<Item>(mockCollection.Object);

        var result = builder.Where(i => i.Name == "Mælk");

        Assert.Same(builder, result);
    }

    // Test at Limit() returnerer samme builder (method chaining)
    [Fact]
    public void QueryBuilder_Limit_ReturnsItself_ForChaining()
    {
        var mockCollection = new Mock<IMongoCollection<Item>>();
        var builder = new QueryBuilder<Item>(mockCollection.Object);

        var result = builder.Limit(10);

        Assert.Same(builder, result);
    }
}
