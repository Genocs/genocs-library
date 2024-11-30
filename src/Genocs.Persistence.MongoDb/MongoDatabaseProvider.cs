using Genocs.Persistence.MongoDb.Configurations;
using Genocs.Persistence.MongoDb.Encryptions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Genocs.Persistence.MongoDb;

/// <summary>
/// The MongoDatabaseProvider.
/// </summary>
public class MongoDatabaseProvider : IMongoDatabaseProvider
{
    /// <summary>
    /// Reference to MongoClient.
    /// </summary>
    public IMongoClient MongoClient { get; private set; }

    /// <summary>
    /// Reference to Database.
    /// </summary>
    public IMongoDatabase Database { get; private set; }

    /// <summary>
    /// Default Constructor.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="encrypOptions"></param>
    /// <exception cref="NullReferenceException">This exception happens in case mandatory data is missing.</exception>
    public MongoDatabaseProvider(IOptions<MongoDbOptions> options, IOptions<MongoDbEncryptionOptions> encrypOptions)
    {
        if (options == null) throw new NullReferenceException(nameof(options));
        MongoDbOptions dBSettings = options.Value;

        if (dBSettings == null) throw new NullReferenceException(nameof(dBSettings));

        if (!MongoDbOptions.IsValid(dBSettings)) throw new InvalidOperationException($"{nameof(dBSettings)} is invalid");

        MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(dBSettings.ConnectionString);

        if (dBSettings.EnableTracing)
        {
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
        }

        /*
        if (encrypOptions != null)
        {
            AzureInitializer initializer = new AzureInitializer();
            var autoEncrypOptions = initializer.EncryptionOptions(encrypOptions);
            clientSettings.AutoEncryptionOptions = autoEncrypOptions;
        }
        */

        this.MongoClient = new MongoClient(clientSettings);
        this.Database = this.MongoClient.GetDatabase(dBSettings.Database);

    }
}
