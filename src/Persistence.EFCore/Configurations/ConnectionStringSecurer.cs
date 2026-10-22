using Genocs.Common.Persistence;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.Options;

namespace Genocs.Persistence.EFCore.Configurations;

public class ConnectionStringSecurer : IConnectionStringSecurer
{
    private readonly IEnumerable<IEFCoreDbProvider> _providers;
    private readonly DatabaseOptions _dbSettings;

    public ConnectionStringSecurer(IEnumerable<IEFCoreDbProvider> providers, IOptions<DatabaseOptions> dbSettings)
    {
        _providers = providers;
        _dbSettings = dbSettings.Value;
    }

    public string? MakeSecure(string? connectionString, string? dbProvider)
    {
        if (connectionString == null || string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        var provider = _providers.TryResolve(dbProvider);

        // Unknown providers leave the connection string untouched.
        return provider is null
            ? connectionString
            : provider.MakeSecureConnectionString(connectionString);
    }
}
