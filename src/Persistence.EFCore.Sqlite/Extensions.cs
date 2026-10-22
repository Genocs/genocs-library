using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.Sqlite;

/// <summary>
/// Registration helpers for the SQLite EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the SQLite database provider (provider key: "sqlite").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddSqliteDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, SqliteDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the SQLite database provider (provider key: "sqlite").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddSqliteDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddSqliteDbProvider();
        return builder;
    }
}
