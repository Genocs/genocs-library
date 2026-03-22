namespace Genocs.Saga.Integrations.Redis.Configurations;

public sealed class SagaRedisOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "sagaRedis";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Redis configuration string.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Redis instance name.
    /// </summary>
    public string? InstanceName { get; set; }
}