using Genocs.APIGateway.Configurations;
using Genocs.Persistence.MongoDb;
using Yarp.ReverseProxy.Configuration;

namespace Genocs.APIGateway.Providers;

public static class MongoDbConfigProviderExtensions
{
    /// <summary>
    /// Adds an InMemoryConfigProvider.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static IReverseProxyBuilder LoadFromDatabase(this IReverseProxyBuilder builder, IConfiguration configuration)
    {
        IConfiguration config = configuration ?? throw new ArgumentNullException("config");

        MongoDbOptions options = new MongoDbOptions();

        config.Bind(MongoDbOptions.Position, options);

        builder.Services.AddSingleton((Func<IServiceProvider, IProxyConfigProvider>)((IServiceProvider sp)
            => new MongodbConfigProvider(sp.GetRequiredService<ILogger<MongodbConfigProvider>>(), sp.GetRequiredService<IMongoDatabaseProvider>(), options)));

        return builder;
    }
}