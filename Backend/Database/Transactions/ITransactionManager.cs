using MongoDB.Driver;

namespace SmartFridgeBackend.Database.Transactions;

// Interface til transaktion-styring.
// Giver os mulighed for at wrappe flere DB-operationer i en atomisk handling.
public interface ITransactionManager : IAsyncDisposable
{
    // Start en ny transaktion, returnerer en session som sendes videre til repository
    Task<IClientSessionHandle> BeginTransactionAsync();

    // Bekræft alle operationer i sessionen
    Task CommitAsync(IClientSessionHandle session);

    // Fortryd alle operationer i sessionen
    Task RollbackAsync(IClientSessionHandle session);
}
