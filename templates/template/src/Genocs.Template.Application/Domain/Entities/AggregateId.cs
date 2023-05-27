namespace Genocs.Template.Application.Domain.Entities;

public class AggregateId : IEquatable<AggregateId>
{
    public Guid Value { get; }

    public AggregateId()
    {
        Value = Guid.NewGuid();
    }

    public AggregateId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// The Equals operator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(AggregateId? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((AggregateId)obj);
    }

    public override int GetHashCode()
        =>  Value.GetHashCode();

    public static implicit operator Guid(AggregateId id)
        => id.Value;

    public static implicit operator AggregateId(Guid id)
        => new AggregateId(id);

    /// <summary>
    /// Return the string data 
    /// </summary>
    /// <returns>string description about the object</returns>
    public override string ToString() 
        => Value.ToString();
}