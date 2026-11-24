namespace Genocs.Persistence.CosmosDB.Domain.Repositories;

/// <summary>
/// Maps entity keys to Cosmos item ids. Item ids must be JSON strings.
/// </summary>
internal static class CosmosItemIdentity
{
    public static void EnsureSupportedKey<TKey>()
    {
        if (typeof(TKey) != typeof(string) && typeof(TKey) != typeof(Guid))
        {
            throw new NotSupportedException(
                $"Cosmos repository key '{typeof(TKey).Name}' is not supported. Use string or Guid so the document id is a JSON string.");
        }
    }

    public static string ToItemId<TKey>(TKey id)
    {
        EnsureSupportedKey<TKey>();

        return id switch
        {
            string value when !string.IsNullOrWhiteSpace(value) => value,
            Guid guid when guid != Guid.Empty => guid.ToString("D"),
            _ => throw new ArgumentException("Document id is required.", nameof(id)),
        };
    }
}
