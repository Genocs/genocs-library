namespace Genocs.Identities.Application.Domain.Entities;

public abstract class AggregateRoot
{
    public AggregateId Id { get; protected set; }
    public int Version { get; protected set; }

    protected AggregateRoot(AggregateId id, int version = 0)
    {
        Id = id;
        Version = version;
    }
}