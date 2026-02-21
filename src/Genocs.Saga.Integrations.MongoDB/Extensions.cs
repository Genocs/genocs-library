using Genocs.Saga.Integrations.MongoDB.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Genocs.Saga.Integrations.MongoDB;

public static class Extensions
{
    private static string DeserializationError => "Could not deserialize given appsettings.";

    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, IConfiguration configuration)
    {
        return builder.UseMongoPersistence(GetDatabase);

        IMongoDatabase GetDatabase(IServiceProvider serviceProvider)
        {
            try
            {
                var mongoSettings = new SagaMongoOption();
                configuration.GetSection(SagaMongoOption.Position).Bind(mongoSettings);
                var database = new MongoClient(mongoSettings.ConnectionString).GetDatabase(mongoSettings.Database);

                return database;
            }
            catch
            {
                throw new SagaException(DeserializationError);
            }
        }
    }

    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, SagaMongoOption settings)
    {
        return builder.UseMongoPersistence(GetDatabase);

        IMongoDatabase GetDatabase(IServiceProvider serviceProvider)
            => new MongoClient(settings.ConnectionString).GetDatabase(settings.Database);
    }

    private static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, Func<IServiceProvider, IMongoDatabase> getDatabase)
    {
        builder.Services.AddTransient(getDatabase);
        builder.UseSagaLog<MongoSagaLog>();
        builder.UseSagaStateRepository<MongoSagaStateRepository>();

        return builder;
    }
}
