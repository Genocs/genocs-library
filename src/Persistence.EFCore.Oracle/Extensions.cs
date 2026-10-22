using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.Oracle;

/// <summary>
/// Registration helpers for the Oracle EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the Oracle database provider (provider key: "oracle").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddOracleDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, OracleDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the Oracle database provider (provider key: "oracle").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddOracleDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddOracleDbProvider();
        return builder;
    }
}
