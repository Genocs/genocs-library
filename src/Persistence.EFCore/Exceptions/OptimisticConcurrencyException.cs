using Genocs.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Genocs.Persistence.EFCore.Exceptions;

/// <summary>
/// Thrown when a write loses an optimistic concurrency check.
/// The stored version no longer matches the version the caller expected.
/// </summary>
public sealed class OptimisticConcurrencyException : ConflictException
{
    public OptimisticConcurrencyException(string message, Type entityType, object? key, long expectedVersion, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        Key = key;
        ExpectedVersion = expectedVersion;
    }

    /// <summary>
    /// CLR type of the aggregate whose write conflicted.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// Primary key of the conflicting aggregate, when the model has one.
    /// </summary>
    public object? Key { get; }

    /// <summary>
    /// Version the write expected to find in the database.
    /// </summary>
    public long ExpectedVersion { get; }

    internal static OptimisticConcurrencyException From(DbUpdateConcurrencyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var entry = exception.Entries.FirstOrDefault();
        if (entry is null)
        {
            return new OptimisticConcurrencyException(
                "Stale write detected. The stored version does not match the expected version.",
                typeof(object),
                key: null,
                expectedVersion: 0,
                exception);
        }

        var key = ReadKey(entry);
        var expectedVersion = ReadExpectedVersion(entry);
        var message = $"Stale write detected for '{entry.Metadata.ClrType.Name}' with id '{key}'. Expected version {expectedVersion}.";
        if (exception.Entries.Count > 1)
        {
            message = $"Stale writes detected for {exception.Entries.Count} entities. {message}";
        }

        return new OptimisticConcurrencyException(message, entry.Metadata.ClrType, key, expectedVersion, exception);
    }

    private static object? ReadKey(EntityEntry entry)
    {
        var properties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (properties is null || properties.Count == 0)
        {
            return null;
        }

        if (properties.Count == 1)
        {
            return entry.Property(properties[0].Name).CurrentValue;
        }

        return string.Join(", ", properties.Select(property => entry.Property(property.Name).CurrentValue));
    }

    private static long ReadExpectedVersion(EntityEntry entry)
    {
        var version = entry.Properties.FirstOrDefault(property => property.Metadata.Name == nameof(IVersioned.Version));
        return version?.OriginalValue is long expected ? expected : 0;
    }
}
