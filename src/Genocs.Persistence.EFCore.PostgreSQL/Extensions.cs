using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.PostgreSQL;

/// <summary>
/// Registration helpers for the PostgreSQL EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the PostgreSQL database provider (provider key: "postgresql").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddPostgreSqlDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, PostgreSqlDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the PostgreSQL database provider (provider key: "postgresql").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddPostgreSqlDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddPostgreSqlDbProvider();
        return builder;
    }
}
