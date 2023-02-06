namespace Genocs.Persistence.MongoDb.Options
{
    /// <summary>
    /// MongoDb database Settings
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// Default Section name
        /// </summary>
        public static string Position = "MongoDb";

        /// <summary>
        /// The Database connection string
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// The database name where the data will be stored
        /// </summary>
        public string Database { get; set; } = default!;

        /// <summary>
        /// Toggle database tracing
        /// </summary>
        public bool EnableTracing { get; set; }


        /// <summary>
        /// Check if the MongoDbSettings object contains valid data
        /// </summary>
        /// <param name="settings">MongoDbSettings object</param>
        /// <returns>return true if valid otherwise false</returns>
        public static bool IsValid(MongoDbSettings settings)
        {
            if (settings is null) return false;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString)) return false;
            if (string.IsNullOrWhiteSpace(settings.Database)) return false;

            return true;

        }
    }
}
