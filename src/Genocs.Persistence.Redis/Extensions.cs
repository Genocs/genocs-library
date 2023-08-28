using Genocs.Core.Builders;
using Genocs.Persistence.Redis.Builders;
using Genocs.Persistence.Redis.Options;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Genocs.Persistence.Redis;


/// <summary>
/// The redis extensions
/// </summary>
public static class Extensions
{
    private const string RegistryName = "persistence.redis";

    /// <summary>
    /// Add Redis
    /// </summary>
    /// <param name="builder">The Genocs builder</param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder, string sectionName = RedisSettings.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = RedisSettings.Position;
        }

        var options = builder.GetOptions<RedisSettings>(sectionName);
        return builder.AddRedis(options);
    }

    /// <summary>
    /// Add Redis
    /// </summary>
    /// <param name="builder">The Genocs builder</param>
    /// <param name="buildOptions"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder,
        Func<IRedisOptionsBuilder, IRedisOptionsBuilder> buildOptions)
    {
        var options = buildOptions(new RedisOptionsBuilder()).Build();
        return builder.AddRedis(options);
    }

    /// <summary>
    /// Add Redis
    /// </summary>
    /// <param name="builder">The Genocs builder</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder, RedisSettings options)
    {
        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services
            .AddSingleton(options)
            .AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(options.ConnectionString))
            .AddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(options.Database))
            .AddStackExchangeRedisCache(o =>
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

        return builder;
    }
}