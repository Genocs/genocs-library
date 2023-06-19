namespace Genocs.Persistence.MongoDb.Options
{
    /// <summary>
    /// MongoDb encryption database Settings
    /// </summary>
    public class MongoDbEncryptionSettings
    {

        /// <summary>
        /// Default Section name
        /// </summary>
        public const string Position = "MongoDbEncryption";

        /// <summary>
        /// The Database connection string
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// The shared library used to encrypt
        /// </summary>
        public string LibPath { get; set; } = default!;

        /// <summary>
        /// Azure Tenant Id
        /// </summary>
        public string TenantId { get; set; } = default!;

        /// <summary>
        /// Azure Client Id
        /// </summary>
        public string ClientId { get; set; } = default!;

        /// <summary>
        /// Azure Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = default!;

        /// <summary>
        /// Azure Client Secret
        /// </summary>
        public string KeyName { get; set; } = default!;

        /// <summary>
        /// Azure Client Secret
        /// </summary>
        public string KeyVersion { get; set; } = default!;

        /// <summary>
        /// Azure Client Secret
        /// </summary>
        public string KeyVaultEndpoint { get; set; } = default!;

        /// <summary>
        /// Check if the MongoDbSettings object contains valid data
        /// </summary>
        /// <param name="settings">MongoDbSettings object</param>
        /// <returns>return true if valid otherwise false</returns>
        public static bool IsValid(MongoDbEncryptionSettings settings)
        {
            if (settings is null) return false;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString)) return false;
            if (string.IsNullOrWhiteSpace(settings.LibPath)) return false;

            return true;

        }
    }
}
