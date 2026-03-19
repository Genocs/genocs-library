using Genocs.Saga.Integrations.Redis.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Genocs.Saga.Integrations.Redis;

public static class Extensions
{
    private static string DeserializationError => "Could not deserialize given appsettings.";

    public static ISagaBuilder UseRedisPersistence(this ISagaBuilder builder, string appSettingsSection, IConfiguration configuration)
    {
        SagaRedisSettings settings;

        try
        {
            settings = JsonConvert.DeserializeObject<SagaRedisSettings>(configuration.GetSection(appSettingsSection)?.Value);
        }
        catch
        {
            throw new SagaException(DeserializationError);
        }

        return builder.ConfigureRedisPersistence(settings);
    }

    public static ISagaBuilder UseRedisPersistence(this ISagaBuilder builder, SagaRedisSettings settings)
    {
        return builder.ConfigureRedisPersistence(settings);
    }

    private static ISagaBuilder ConfigureRedisPersistence(this ISagaBuilder builder, SagaRedisSettings settings)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = settings.Configuration;
            options.InstanceName = settings.InstanceName;
        });

        builder.UseSagaLog<RedisSagaLog>();
        builder.UseSagaStateRepository<RedisSagaStateRepository>();

        return builder;
    }
}