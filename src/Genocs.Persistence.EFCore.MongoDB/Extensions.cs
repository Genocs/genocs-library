using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.MongoDB;

/// <summary>
/// Registration helpers for the MongoDB EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the MongoDB database provider (provider key: "mongodb").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMongoDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, MongoDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the MongoDB database provider (provider key: "mongodb").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddMongoDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddMongoDbProvider();
        return builder;
    }
}
