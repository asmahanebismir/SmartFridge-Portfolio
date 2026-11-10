using MongoDB.Driver;

namespace SmartFridgeBackend.Database.Repositories;

// Generisk repository-interface - kontrakt for alle CRUD-operationer.
// T er den entitetstype repositoryet arbejder med (fx User, Item, Recipe).
// where T : class sikrer at T kun kan være en referencetype (klasse).
public interface IRepository<T> where T : class
{
    // Hent et dokument på ObjectId-streng
    Task<T?> GetByIdAsync(string id);

    // Hent alle dokumenter i samlingen
    Task<List<T>> GetAllAsync();

    // Hent dokumenter der opfylder et filter (LINQ expression)
    Task<List<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter);

    // Tilføj et nyt dokument
    Task InsertAsync(T entity);

    // Tilføj et nyt dokument inde i en eksisterende session (transaktion)
    Task InsertAsync(T entity, IClientSessionHandle session);

    // Erstat et dokument (upsert = indsæt hvis det ikke findes)
    Task<bool> ReplaceAsync(string id, T entity);

    // Slet et dokument
    Task<bool> DeleteAsync(string id);
}
