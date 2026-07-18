using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Providers;

internal sealed class SqlServerDbProvider : IEFCoreDbProvider
{
    private const string HiddenValueDefault = "*******";

    public string ProviderKey => DbProviderKeys.SqlServer;

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseSqlServer(options.ConnectionString, e =>
            e.MigrationsAssembly("Migrators.MSSQL"));

    public string MakeSecureConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) || !builder.IntegratedSecurity)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID) || !builder.IntegratedSecurity)
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    public bool TryValidateConnectionString(string connectionString)
    {
        _ = new SqlConnectionStringBuilder(connectionString);
        return true;
    }
}
