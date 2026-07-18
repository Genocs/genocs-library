using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Sqlite;

/// <summary>
/// SQLite implementation of <see cref="IEFCoreDbProvider"/>.
/// </summary>
public sealed class SqliteDbProvider : IEFCoreDbProvider
{
    public string ProviderKey => DbProviderKeys.SqLite;

    public bool SupportsMigrations => true;

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
