using Genocs.Common.Persistence;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Persistence.EFCore.Configurations;

internal class ConnectionStringValidator : IConnectionStringValidator
{
    private readonly IEnumerable<IEFCoreDbProvider> _providers;
    private readonly DatabaseOptions _dbSettings;
    private readonly ILogger<ConnectionStringValidator> _logger;

    public ConnectionStringValidator(
        IEnumerable<IEFCoreDbProvider> providers,
        IOptions<DatabaseOptions> dbSettings,
        ILogger<ConnectionStringValidator> logger)
    {
        _providers = providers;
        _dbSettings = dbSettings.Value;
        _logger = logger;
    }

    public bool TryValidate(string connectionString, string? dbProvider = null)
    {
        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        try
        {
            var provider = _providers.TryResolve(dbProvider);

            // Unknown providers are considered valid (historic fall-through behavior).
            return provider is null || provider.TryValidateConnectionString(connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection String Validation Exception : {ex.Message}");
            return false;
        }
    }
}
