using System.Linq.Expressions;
using MongoDB.Driver;

namespace SmartFridgeBackend.Database.Query;

// Fluent query builder, lader os bygge forespørgsler
// med metode-kæder (method chaining) frem for rå MongoDB filter-syntax.

public class QueryBuilder<T> where T : class
{
    private readonly IMongoCollection<T> _collection;
    private FilterDefinition<T> _filter = Builders<T>.Filter.Empty;
    private SortDefinition<T>? _sort;
    private int? _limit;
    private int _skip = 0;

    public QueryBuilder(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    // Tilføj et LINQ where-filter
    public QueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _filter = Builders<T>.Filter.Where(predicate);
        return this;  // returnerer this til method chaining
    }

    // Sorter stigende på et felt
    public QueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var fieldName = GetFieldName(keySelector);
        _sort = Builders<T>.Sort.Ascending(fieldName);
        return this;
    }

    // Sorter faldende på et felt
    public QueryBuilder<T> OrderByDescending<TKey>(
        Expression<Func<T, TKey>> keySelector)
    {
        var fieldName = GetFieldName(keySelector);
        _sort = Builders<T>.Sort.Descending(fieldName);
        return this;
    }

    // Begræns antal resultater
    public QueryBuilder<T> Limit(int count)
    {
        _limit = count;
        return this;
    }

    // Spring de første n resultater over (til pagination)
    public QueryBuilder<T> Skip(int count)
    {
        _skip = count;
        return this;
    }

    // Udfør forespørgslen og returner listen
    public async Task<List<T>> ExecuteAsync()
    {
        var query = _collection.Find(_filter);

        if (_sort != null)
            query = query.Sort(_sort);

        if (_skip > 0)
            query = query.Skip(_skip);

        if (_limit.HasValue)
            query = query.Limit(_limit.Value);

        return await query.ToListAsync();
    }

    // Tæl antal matchende dokumenter
    public async Task<long> CountAsync()
    {
        return await _collection.CountDocumentsAsync(_filter);
    }

    private static string GetFieldName<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        if (keySelector.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (keySelector.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression unaryMemberExpression)
            return unaryMemberExpression.Member.Name;

        throw new ArgumentException("Ugyldigt feltudtryk", nameof(keySelector));
    }
}