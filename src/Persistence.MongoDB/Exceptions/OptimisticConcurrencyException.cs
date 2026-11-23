namespace Genocs.Persistence.MongoDB.Exceptions;

/// <summary>
/// Thrown when a MongoDB write loses an optimistic concurrency check.
/// The stored <c>Version</c> no longer matches the version the caller expected.
/// </summary>
public sealed class OptimisticConcurrencyException : Exception
{
    /// <summary>
    /// Creates an exception for a stale versioned write.
    /// </summary>
    /// <param name="message">Conflict description.</param>
    /// <param name="entityType">CLR type of the document whose write conflicted.</param>
    /// <param name="key">Primary key of the conflicting document.</param>
    /// <param name="expectedVersion">Version the write expected to find in the collection.</param>
    /// <param name="innerException">Driver exception that exposed the conflict, when there is one.</param>
    public OptimisticConcurrencyException(string message, Type entityType, object? key, long expectedVersion, Exception? innerException = null)
        : base(message, innerException)
    {
        EntityType = entityType;
        Key = key;
        ExpectedVersion = expectedVersion;
    }

    /// <summary>
    /// CLR type of the document whose write conflicted.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// Primary key of the conflicting document.
    /// </summary>
    public object? Key { get; }

    /// <summary>
    /// Version the write expected to find in the collection.
    /// </summary>
    public long ExpectedVersion { get; }

    internal static OptimisticConcurrencyException For(Type entityType, object? key, long expectedVersion, Exception? innerException = null)
    {
        var message = $"Stale write detected for '{entityType.Name}' with id '{key}'. Expected version {expectedVersion}.";
        return new OptimisticConcurrencyException(message, entityType, key, expectedVersion, innerException);
    }
}
