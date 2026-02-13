using Genocs.Common.Domain.Entities;

namespace Genocs.Common.Domain.Entities.Auditing;

/// <summary>
/// An entity can implement this interface if <see cref="DeletedAt"/> of this entity must be stored.
/// <see cref="DeletedAt"/> is automatically set when deleting <see cref="Entity"/>.
/// </summary>
public interface IHasDeletionTime : ISoftDelete
{
    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
