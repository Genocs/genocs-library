using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.SqlServer;

/// <summary>
/// Registration helpers for the SQL Server EF Core provider.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the SQL Server database provider (provider key: "mssql").
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerDbProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEFCoreDbProvider, SqlServerDbProvider>());
        return services;
    }

    /// <summary>
    /// Registers the SQL Server database provider (provider key: "mssql").
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddSqlServerDbProvider(this IGenocsBuilder builder)
    {
        builder.Services.AddSqlServerDbProvider();
        return builder;
    }
}
