using MongoDB.Bson;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Exceptions;

namespace SmartFridgeBackend.Database.Repositories;

// Konkret MongoDB-implementering af IRepository<T>.
// IMongoDatabase injiceres via constructor (Dependency Injection).
public class MongoRepository<T> : IRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;
    private readonly string _collectionName;

    // collectionName = navnet på MongoDB-samlingen (fx "Users", "Items")
    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collectionName = collectionName;
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (FormatException)
        {
            throw new RecordNotFoundException(_collectionName, id);
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<List<T>> FindAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity);
        }
        catch (MongoWriteException ex) when (
            ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateKeyException(
                $"Duplikat nøgle ved insert i '{_collectionName}'", ex);
        }
    }

    // Overload med session, bruges inde i transaktioner
    public async Task InsertAsync(T entity, IClientSessionHandle session)
    {
        try
        {
            await _collection.InsertOneAsync(session, entity);
        }
        catch (MongoWriteException ex) when (
            ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateKeyException(
                $"Duplikat nøgle ved insert i '{_collectionName}'", ex);
        }
    }

    public async Task<bool> ReplaceAsync(string id, T entity)
    {
        var objectId = ObjectId.Parse(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var result = await _collection.ReplaceOneAsync(
            filter, entity, new ReplaceOptions { IsUpsert = false });
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var result = await _collection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
                throw new RecordNotFoundException(_collectionName, id);

            return true;
        }
        catch (FormatException)
        {
            throw new RecordNotFoundException(_collectionName, id);
        }
    }
}
