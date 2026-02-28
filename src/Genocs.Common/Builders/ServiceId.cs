namespace Genocs.Common.Builders;

/// <summary>
/// The ServiceId class is an implementation of the IServiceId interface.
/// It generates a unique identifier for a service using a GUID (Globally Unique Identifier).
/// The Id property returns the generated identifier as a string.
/// This class can be used to assign unique IDs to services in a consistent manner across an application.
/// </summary>
public class ServiceId : IServiceId
{
    /// <summary>
    /// Gets the unique identifier for this instance.
    /// </summary>
    /// <remarks>The identifier is generated as a new GUID and formatted as a string without hyphens. Each
    /// instance receives a distinct value when created.</remarks>
    public string Id { get; } = $"{Guid.NewGuid():N}";
}