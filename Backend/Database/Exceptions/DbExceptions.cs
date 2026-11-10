namespace SmartFridgeBackend.Database.Exceptions;

// Basis-exception for alle database-fejl
public class DbException : Exception
{
    public DbException(string message) : base(message) { }
    public DbException(string message, Exception inner) : base(message, inner) { }
}

// Kastes når et dokument ikke findes (svarende til HTTP 404)
public class RecordNotFoundException : DbException
{
    public string Collection { get; }
    public string Id { get; }

    public RecordNotFoundException(string collection, string id)
        : base($"Dokument '{id}' ikke fundet i samling '{collection}'")
    {
        Collection = collection;
        Id = id;
    }
}

// Kastes ved duplikat-nøgle violation (svarende til HTTP 409)
public class DuplicateKeyException : DbException
{
    public DuplicateKeyException(string message) : base(message) { }
    public DuplicateKeyException(string message, Exception inner) : base(message, inner) { }
}

// Kastes ved fejl under en transaktion
public class TransactionException : DbException
{
    public TransactionException(string message) : base(message) { }
    public TransactionException(string message, Exception inner) : base(message, inner) { }
}
