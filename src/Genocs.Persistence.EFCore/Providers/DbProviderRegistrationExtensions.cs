using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Persistence.EFCore.Providers;

internal static class DbProviderRegistrationExtensions
{
    /// <summary>
    /// Registers all built-in EF Core database providers.
    /// Registration is idempotent so it can be invoked from multiple entry points
    /// (e.g. AddEFCorePersistence and AddFinbuckleMultiTenancyWithEfCoreStore).
    /// </summary>
    public static IServiceCollection AddEFCoreDbProviders(this IServiceCollection services)
    {
        services.TryAddEnumerable(
        [
            ServiceDescriptor.Singleton<IEFCoreDbProvider, SqlServerDbProvider>(),
            ServiceDescriptor.Singleton<IEFCoreDbProvider, PostgreSqlDbProvider>(),
            ServiceDescriptor.Singleton<IEFCoreDbProvider, MySqlDbProvider>(),
            ServiceDescriptor.Singleton<IEFCoreDbProvider, SqliteDbProvider>(),
            ServiceDescriptor.Singleton<IEFCoreDbProvider, OracleDbProvider>(),
            ServiceDescriptor.Singleton<IEFCoreDbProvider, MongoDbProvider>(),
        ]);

        return services;
    }
}
