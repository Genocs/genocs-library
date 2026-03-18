using Genocs.APIGateway.WebApi.Configurations;
using Genocs.Persistence.MongoDB;
using Yarp.ReverseProxy.Configuration;

namespace Genocs.APIGateway.WebApi.Providers;

public static class MongoDbConfigProviderExtensions
{
    /// <summary>
    /// Adds a MongoDB-based configuration provider to the reverse proxy builder, allowing it to read its configuration from a MongoDB database.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is null.</exception>
    public static IReverseProxyBuilder LoadFromDatabase(this IReverseProxyBuilder builder, IConfiguration configuration)
    {
        var config = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var options = new YarpMongoDbOptions();

        config.Bind(YarpMongoDbOptions.Position, options);

        builder.Services.AddSingleton((Func<IServiceProvider, IProxyConfigProvider>)((sp)
            => new MongodbConfigProvider(sp.GetRequiredService<ILogger<MongodbConfigProvider>>(), sp.GetRequiredService<IMongoDatabaseProvider>(), options)));

        return builder;
    }
}