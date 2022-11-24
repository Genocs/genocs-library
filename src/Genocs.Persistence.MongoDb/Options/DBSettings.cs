namespace Genocs.Persistence.MongoDb.Options
{
    /// <summary>
    /// MongoDb database Settings
    /// </summary>
    public class DBSettings
    {
        /// <summary>
        /// Default Section name
        /// </summary>
        public static string Position { get; set; } = "DBSettings";

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
    }
}
