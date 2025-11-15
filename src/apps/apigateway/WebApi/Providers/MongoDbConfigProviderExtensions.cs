using Genocs.APIGateway.WebApi.Configurations;
using Genocs.Persistence.MongoDb;
using Yarp.ReverseProxy.Configuration;

namespace Genocs.APIGateway.WebApi.Providers;

public static class MongoDbConfigProviderExtensions
{
    /// <summary>
    /// Adds an InMemoryConfigProvider.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static IReverseProxyBuilder LoadFromDatabase(this IReverseProxyBuilder builder, IConfiguration configuration)
    {
        var config = configuration ?? throw new ArgumentNullException("config");

        var options = new YarpMongoDbOptions();

        config.Bind(YarpMongoDbOptions.Position, options);

        builder.Services.AddSingleton((Func<IServiceProvider, IProxyConfigProvider>)((sp)
            => new MongodbConfigProvider(sp.GetRequiredService<ILogger<MongodbConfigProvider>>(), sp.GetRequiredService<IMongoDatabaseProvider>(), options)));

        return builder;
    }
}