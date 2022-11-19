namespace Genocs.Persistence.MongoDb
{
    using Genocs.Persistence.MongoDb.Options;
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using System;

    public class MongoDatabaseProvider : IMongoDatabaseProvider
    {
        public IMongoDatabase Database { get; set; }

        public MongoDatabaseProvider(IOptions<DBSettings> options)
        {
            if (options == null) throw new NullReferenceException(nameof(options));
            DBSettings dBSettings = options.Value;

            if (dBSettings == null) throw new NullReferenceException(nameof(dBSettings));

            MongoClient client = new MongoClient(dBSettings.ConnectionString);
            Database = client.GetDatabase(dBSettings.Database);
        }
    }
}
