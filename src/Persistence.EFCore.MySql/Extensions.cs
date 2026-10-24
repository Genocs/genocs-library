using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.MySql;

/// <summary>
/// Registration helpers for the MySQL EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the MySQL database provider (provider key: "mysql").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMySqlDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, MySqlDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the MySQL database provider (provider key: "mysql").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddMySqlDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddMySqlDbProvider();
        return builder;
    }
}
