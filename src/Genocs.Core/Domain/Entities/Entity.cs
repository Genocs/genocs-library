﻿using System.Reflection;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// A shortcut of <see cref="Entity{TPrimaryKey}"/> for most used primary key type (<see cref="DefaultIdType"/>).
/// </summary>
[Serializable]
public abstract class Entity : Entity<DefaultIdType>;

/// <summary>
/// Basic implementation of IEntity interface.
/// An entity can inherit this class of directly implement to IEntity interface.
/// </summary>
/// <typeparam name="TPrimaryKey">Type of the primary key of the entity.</typeparam>
[Serializable]
public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public virtual TPrimaryKey Id { get; set; } = default!;

    /// <summary>
    /// Checks if this entity is transient (it has not an Id).
    /// </summary>
    /// <returns>True, if this entity is transient.</returns>
    public virtual bool IsTransient()
    {
        if (EqualityComparer<TPrimaryKey>.Default.Equals(Id!, default!))
        {
            return true;
        }

        // Workaround for EF Core since it sets int/long to min value when attaching to dB context
        if (typeof(TPrimaryKey) == typeof(int))
        {
            return Convert.ToInt32(Id) <= 0;
        }

        if (typeof(TPrimaryKey) == typeof(long))
        {
            return Convert.ToInt64(Id) <= 0;
        }

        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity<TPrimaryKey>)
        {
            return false;
        }

        // Same instances must be considered as equal
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        // Transient objects are not considered as equal
        var other = (Entity<TPrimaryKey>)obj;
        if (IsTransient() && other.IsTransient())
        {
            return false;
        }

        // Must have a IS-A relation of types or must be same type
        var typeOfThis = GetType();
        var typeOfOther = other.GetType();
        if (!typeOfThis.GetTypeInfo().IsAssignableFrom(typeOfOther) && !typeOfOther.GetTypeInfo().IsAssignableFrom(typeOfThis))
        {
            return false;
        }

        /*
        if (this is IMayHaveTenant && other is IMayHaveTenant &&
            this.As<IMayHaveTenant>().TenantId != other.As<IMayHaveTenant>().TenantId)
        {
            return false;
        }

        if (this is IMustHaveTenant && other is IMustHaveTenant &&
            this.As<IMustHaveTenant>().TenantId != other.As<IMustHaveTenant>().TenantId)
        {
            return false;
        }
        */
        return Id!.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (Id == null)
        {
            return 0;
        }

        return Id.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right)
    {
        if (Equals(left, null))
        {
            return Equals(right, null);
        }

        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{GetType().Name} {Id}]";
    }
}
