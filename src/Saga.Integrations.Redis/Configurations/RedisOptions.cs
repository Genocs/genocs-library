namespace Genocs.Saga.Integrations.Redis.Configurations;

public sealed class RedisOptions
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
    /// The connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost";

    /// <summary>
    /// Redis instance.
    /// </summary>
    public string Instance { get; set; }

    /// <summary>
    /// The database Id.
    /// </summary>
    public int Database { get; set; }

    /// <summary>
    /// Check if the MongoDbSettings object contains valid data.
    /// </summary>
    /// <param name="settings">MongoDbSettings object.</param>
    /// <returns>return true if valid otherwise false.</returns>
    public static bool IsValid(RedisOptions settings)
    {
        if (settings is null) return false;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString)) return false;
        if (string.IsNullOrWhiteSpace(settings.Instance)) return false;

        return true;
    }
}