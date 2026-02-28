namespace Genocs.Persistence.MongoDB.Configurations;

/// <summary>
/// MongoDB encryption database Settings.
/// </summary>
public class MongoEncryptionOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "mongoDbEncryption";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The Database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// The shared library used to encrypt.
    /// </summary>
    public string LibPath { get; set; } = default!;

    /// <summary>
    /// Azure Tenant Id.
    /// </summary>
    public string TenantId { get; set; } = default!;

    /// <summary>
    /// Azure Client Id.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Azure Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = default!;

    /// <summary>
    /// Azure Client Secret.
    /// </summary>
    public string KeyName { get; set; } = default!;

    /// <summary>
    /// Azure Client Secret.
    /// </summary>
    public string KeyVersion { get; set; } = default!;

    /// <summary>
    /// Azure Client Secret.
    /// </summary>
    public string KeyVaultEndpoint { get; set; } = default!;

    /// <summary>
    /// Check if the MongoDbSettings object contains valid data.
    /// </summary>
    /// <param name="settings">MongoDbSettings object.</param>
    /// <returns>return true if valid otherwise false.</returns>
    public static bool IsValid(MongoEncryptionOptions settings)
    {
        if (settings is null) return false;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString)) return false;
        if (string.IsNullOrWhiteSpace(settings.LibPath)) return false;

        return true;

    }
}
