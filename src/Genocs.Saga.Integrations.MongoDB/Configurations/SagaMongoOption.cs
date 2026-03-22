namespace Genocs.Saga.Integrations.MongoDB.Configurations;

/// <summary>
/// Represents the options for configuring MongoDB integration in Genocs Saga.
/// </summary>
public sealed class SagaMongoOptions
{
        /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "sagaMongo";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
}
