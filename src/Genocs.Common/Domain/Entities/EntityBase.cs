namespace Genocs.Common.Domain.Entities;

/// <summary>
/// Optional base entity implementation with identity-based equality semantics.
/// </summary>
/// <typeparam name="TKey">Type of the entity identifier.</typeparam>
public abstract class EntityBase<TKey> : IEntity<TKey>, IEquatable<EntityBase<TKey>>
{
    /// <summary>
    /// Gets the identifier of this entity.
    /// </summary>
    public virtual TKey Id { get; protected set; } = default!;

    /// <summary>
    /// Checks if this entity is transient (it has not a persisted identity yet).
    /// </summary>
    /// <returns>True, if this entity is transient.</returns>
    public virtual bool IsTransient()
    {
        if (EqualityComparer<TKey>.Default.Equals(Id!, default!))
        {
            return true;
        }

        // Keeps compatibility with common numeric-key persistence conventions.
        if (typeof(TKey) == typeof(int))
        {
            return Convert.ToInt32(Id) <= 0;
        }

        if (typeof(TKey) == typeof(long))
        {
            return Convert.ToInt64(Id) <= 0;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool Equals(EntityBase<TKey>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (IsTransient() && other.IsTransient())
        {
            return false;
        }

        Type typeOfThis = GetType();
        Type typeOfOther = other.GetType();
        if (!typeOfThis.IsAssignableFrom(typeOfOther) && !typeOfOther.IsAssignableFrom(typeOfThis))
        {
            return false;
        }

        return EqualityComparer<TKey>.Default.Equals(Id!, other.Id!);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is EntityBase<TKey> other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (Id is null)
        {
            return 0;
        }

        return EqualityComparer<TKey>.Default.GetHashCode(Id);
    }

    /// <summary>
    /// Compares two entities for equality.
    /// </summary>
    public static bool operator ==(EntityBase<TKey> left, EntityBase<TKey> right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Compares two entities for inequality.
    /// </summary>
    public static bool operator !=(EntityBase<TKey> left, EntityBase<TKey> right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{GetType().Name} {Id}]";
    }
}
