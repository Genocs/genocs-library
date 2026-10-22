using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Saga.Integrations.Redis.Configurations;
using Genocs.Saga.Integrations.Redis.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Genocs.Saga.Integrations.Redis;

public static class Extensions
{
    private static string DeserializationError => "Could not deserialize given appsettings.";

    public static ISagaBuilder UseRedisPersistence(this ISagaBuilder builder, IConfiguration configuration, string sectionName = RedisOptions.Position)
    {
        RedisOptions settings = ResolveSettings(configuration, sectionName);

        try
        {

            return builder.ConfigureRedisPersistence(settings);

        }
        catch
        {
            throw new SagaException(DeserializationError);
        }
    }

    public static ISagaBuilder UseRedisPersistence(this ISagaBuilder builder, RedisOptions settings)
    {
        return builder.ConfigureRedisPersistence(settings);
    }

    private static RedisOptions ResolveSettings(IConfiguration configuration, string sectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = RedisOptions.Position;
        }

        var settings = configuration.GetOptions<RedisOptions>(sectionName);
        if (RedisOptions.IsValid(settings))
        {
            return settings;
        }

        throw new InvalidConfigurationException(DeserializationError);
    }

    private static ISagaBuilder ConfigureRedisPersistence(this ISagaBuilder builder, RedisOptions settings)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = settings.ConnectionString;
            options.InstanceName = settings.Instance;
        });

        builder.Services
            .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(settings.ConnectionString!))
            .AddSingleton(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase())
            .AddSingleton<IRedisSagaStateStore>(sp => new RedisSagaStateStore(
                sp.GetRequiredService<IDatabase>(),
                settings.Instance));

        builder.UseSagaLog<RedisSagaLog>();
        builder.UseSagaStateRepository<RedisSagaStateRepository>();

        return builder;
    }
}