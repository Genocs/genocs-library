namespace Genocs.Persistence.MongoDb.Options
{
    public class DBSettings
    {
        public static string Position { get; set; } = "DBSettings";

        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = default!;
    }
}
