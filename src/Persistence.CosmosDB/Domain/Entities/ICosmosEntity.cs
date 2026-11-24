using Genocs.Common.Domain.Entities;

namespace Genocs.Persistence.CosmosDB.Domain.Entities;

/// <summary>
/// Cosmos document with a string partition key stored as <c>partitionKey</c>.
/// Containers registered with this package must use the partition key path <c>/partitionKey</c>.
/// Supported key types are <see cref="string"/> and <see cref="Guid"/>, because Cosmos item ids are JSON strings.
/// </summary>
/// <typeparam name="TKey">Document id type.</typeparam>
public interface ICosmosEntity<TKey> : IEntity<TKey>
{
    /// <summary>
    /// Logical partition value for point reads, writes, and deletes.
    /// </summary>
    string PartitionKey { get; }
}
