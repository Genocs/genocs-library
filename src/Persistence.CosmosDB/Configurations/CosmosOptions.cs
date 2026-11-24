namespace Genocs.Persistence.CosmosDB.Configurations;

/// <summary>
/// Azure Cosmos DB connection settings. Bind from the <c>cosmosDb</c> configuration section.
/// </summary>
public class CosmosOptions
{
    /// <summary>
    /// Default configuration section name.
    /// </summary>
    public const string Position = "cosmosDb";

    /// <summary>
    /// Cosmos DB connection string (AccountEndpoint and AccountKey).
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// Database id that holds the containers used by repositories.
    /// </summary>
    public string Database { get; set; } = default!;

    /// <summary>
    /// Returns whether both the connection string and database id are present.
    /// </summary>
    /// <param name="settings">Options instance to validate.</param>
    /// <returns><c>true</c> when the options can be used to open a client.</returns>
    public static bool IsValid(CosmosOptions? settings)
    {
        if (settings is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.Database))
        {
            return false;
        }

        return true;
    }
}
