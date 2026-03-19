using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Encryptions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Genocs.Persistence.MongoDB;

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
    public MongoDatabaseProvider(IOptions<MongoOptions> options, IOptions<MongoEncryptionOptions> encrypOptions)
    {
        if (options == null) throw new NullReferenceException(nameof(options));
        MongoOptions dBSettings = options.Value;

        if (dBSettings == null) throw new NullReferenceException(nameof(dBSettings));

        if (!MongoOptions.IsValid(dBSettings)) throw new InvalidOperationException($"{nameof(dBSettings)} is invalid");

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
