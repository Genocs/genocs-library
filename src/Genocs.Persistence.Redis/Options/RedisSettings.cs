namespace Genocs.Persistence.Redis.Options;

/// <summary>
/// The Redis Options
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// The section name
    /// </summary>
    public const string Position = "redis";

    /// <summary>
    /// The connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost";

    /// <summary>
    /// Redis instance
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// The database Id
    /// </summary>
    public int Database { get; set; }
}