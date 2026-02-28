using Genocs.Core.Builders;
using Genocs.Persistence.Redis.Builders;
using Genocs.Persistence.Redis.Configurations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Genocs.Persistence.Redis;

/// <summary>
/// The redis extensions.
/// </summary>
public static class Extensions
{
    private const string RegistryName = "persistence.redis";

    /// <summary>
    /// Add Redis support by means of Genocs builder.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The Genocs builder. You can use it to chain commands.</returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder, string sectionName = RedisOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = RedisOptions.Position;
        }

        var options = builder.GetOptions<RedisOptions>(sectionName);
        return builder.AddRedis(options);
    }

    /// <summary>
    /// Add Redis support by means of Genocs builder.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="buildOptions">A function to configure the Redis options.</param>
    /// <returns>The Genocs builder. You can use it to chain commands.</returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder, Func<IRedisOptionsBuilder, IRedisOptionsBuilder> buildOptions)
    {
        var options = buildOptions(new RedisSettingsBuilder()).Build();
        return builder.AddRedis(options);
    }

    /// <summary>
    /// Add Redis support by means of Genocs builder.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="options">The Redis options.</param>
    /// <returns>The Genocs builder. You can use it to chain commands.</returns>
    public static IGenocsBuilder AddRedis(this IGenocsBuilder builder, RedisOptions options)
    {
        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services
            .AddSingleton(options)
            .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options.ConnectionString))
            .AddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(options.Database))
            .AddStackExchangeRedisCache(o =>
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

        return builder;
    }
}