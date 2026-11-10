using MongoDB.Driver;
using SmartFridgeBackend.Database.Exceptions;

namespace SmartFridgeBackend.Database.Transactions;

public class MongoTransactionManager : ITransactionManager
{
    private readonly IMongoClient _client;

    public MongoTransactionManager(IMongoClient client)
    {
        _client = client;
    }

    public async Task<IClientSessionHandle> BeginTransactionAsync()
    {
        var session = await _client.StartSessionAsync();
        session.StartTransaction();
        return session;
    }

    public async Task CommitAsync(IClientSessionHandle session)
    {
        try
        {
            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            throw new TransactionException("Commit fejlede", ex);
        }
    }

    public async Task RollbackAsync(IClientSessionHandle session)
    {
        try
        {
            await session.AbortTransactionAsync();
        }
        catch (Exception ex)
        {
            throw new TransactionException("Rollback fejlede", ex);
        }
    }

    // IAsyncDisposable, sikrer at sessionen lukkes korrekt
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
