namespace Genocs.Common.Domain.Entities;

/// <summary>
/// Defines a minimal optimistic concurrency contract.
/// </summary>
/// <remarks>
/// Implementations should increase the version whenever a persisted state change is accepted.
/// The persistence adapter is responsible for enforcing compare-and-swap semantics.
/// </remarks>
public interface IVersioned
{
    /// <summary>
    /// Gets the current aggregate or entity version used for optimistic concurrency checks.
    /// </summary>
    long Version { get; }
}