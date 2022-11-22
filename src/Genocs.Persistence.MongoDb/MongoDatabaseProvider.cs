namespace Genocs.Persistence.MongoDb
{
    using Genocs.Persistence.MongoDb.Options;
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Extensions.DiagnosticSources;
    using System;

    /// <summary>
    /// The MongoDatabaseProvider
    /// </summary>
    public class MongoDatabaseProvider : IMongoDatabaseProvider
    {
        /// <summary>
        /// Reference to Database
        /// </summary>
        public IMongoDatabase Database { get; set; }

        /// <summary>
        /// Default Constractor
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="NullReferenceException"></exception>
        public MongoDatabaseProvider(IOptions<DBSettings> options)
        {
            if (options == null) throw new NullReferenceException(nameof(options));
            DBSettings dBSettings = options.Value;

            if (dBSettings == null) throw new NullReferenceException(nameof(dBSettings));

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(dBSettings.ConnectionString);

            if (dBSettings.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }
            MongoClient mongoClient = new MongoClient(clientSettings);

            Database = mongoClient.GetDatabase(dBSettings.Database);
        }
    }
}
