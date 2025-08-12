using Genocs.Common.Types;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// Defines interface for base entity type. All the domain object must implement this interface.
/// </summary>
/// <typeparam name="TKey">Type of the primary key of the entity.</typeparam>
public interface IEntity<TKey> : IEntity, IIdentifiable<TKey>;
