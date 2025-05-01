namespace Genocs.Core.Domain.Entities.Auditing;

/// <summary>
/// The type of trail.
/// </summary>
public enum TrailType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}