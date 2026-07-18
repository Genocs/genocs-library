using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Providers;

internal sealed class SqliteDbProvider : IEFCoreDbProvider
{
    public string ProviderKey => DbProviderKeys.SqLite;

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseSqlite(options.ConnectionString, e =>
            e.MigrationsAssembly("Migrators.SqLite"));

    public string MakeSecureConnectionString(string connectionString)
    {
        // SQLite connection strings carry no credentials to mask.
        var builder = new SqliteConnection(connectionString);

        return builder.ToString();
    }

    public bool TryValidateConnectionString(string connectionString)
    {
        _ = new SqliteConnection(connectionString);
        return true;
    }
}
