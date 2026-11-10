using Moq;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Repositories;
using SmartFridgeBackend.Database.Exceptions;
using SmartFridgeBackend.Database.Transactions;

namespace SmartFridgeBackend.BackendTests;

public class RepositoryTests
{
    // Test at GetAllAsync returnerer en liste (kan være tom)
    [Fact]
    public async Task GetAllAsync_ReturnsListOfUsers()
    {
        // Arrange – opsæt mock database og collection
        var mockCollection = new Mock<IMongoCollection<User>>();
        var mockDatabase  = new Mock<IMongoDatabase>();
        var mockCursor    = new Mock<IAsyncCursor<User>>();

        var users = new List<User>
        {
            new User { UserId = null, Email = "a@b.com", AuthId = "uid1" }
        };

        // Sæt mock op til at returnere users-listen
        mockCursor.SetupSequence(c => c.MoveNextAsync(default))
                  .ReturnsAsync(true).ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(users);

        mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                default))
            .ReturnsAsync(mockCursor.Object);

        mockDatabase.Setup(db => db.GetCollection<User>(
                It.IsAny<string>(),
                It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCollection.Object);

        // Act – kald den metode vi tester
        var repo = new MongoRepository<User>(mockDatabase.Object, "Users");
        var result = await repo.GetAllAsync();

        // Assert – verificer resultatet
        Assert.NotNull(result);
    }

    // Test at GetByIdAsync kaster RecordNotFoundException ved ugyldigt ID
    [Fact]
    public async Task GetByIdAsync_InvalidId_ThrowsRecordNotFoundException()
    {
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockCollection = new Mock<IMongoCollection<User>>();

        mockDatabase.Setup(db => db.GetCollection<User>(
                It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCollection.Object);

        var repo = new MongoRepository<User>(mockDatabase.Object, "Users");

        // "ikke-et-valid-objectid" er ikke et gyldigt MongoDB ObjectId
        await Assert.ThrowsAsync<RecordNotFoundException>(
            () => repo.GetByIdAsync("ikke-et-valid-objectid"));
    }
}
