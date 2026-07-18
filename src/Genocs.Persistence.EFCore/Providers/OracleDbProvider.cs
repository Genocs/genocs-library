using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Genocs.Persistence.EFCore.Providers;

internal sealed class OracleDbProvider : IEFCoreDbProvider
{
    private const string HiddenValueDefault = "*******";

    public string ProviderKey => DbProviderKeys.Oracle;

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseOracle(options.ConnectionString, e =>
            e.MigrationsAssembly("Migrators.Oracle"));

    public string MakeSecureConnectionString(string connectionString)
    {
        var builder = new OracleConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    public bool TryValidateConnectionString(string connectionString)
        => true;
}
