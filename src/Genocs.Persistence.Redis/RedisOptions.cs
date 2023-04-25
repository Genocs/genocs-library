namespace Genocs.Persistence.Redis;

/// <summary>
/// The Redis Options
/// </summary>
public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
    public string? Instance { get; set; }
    public int Database { get; set; }
}