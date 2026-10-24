using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Genocs.Persistence.EFCore.PostgreSQL;

/// <summary>
/// PostgreSQL implementation of <see cref="IEFCoreDbProvider"/>.
/// </summary>
public sealed class PostgreSqlDbProvider : IEFCoreDbProvider
{
    private const string HiddenValueDefault = "*******";

    public string ProviderKey => DbProviderKeys.Npgsql;

    public bool SupportsMigrations => true;

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseNpgsql(options.ConnectionString, e =>
            e.MigrationsAssembly("Migrators.PostgreSQL"));

    public string MakeSecureConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) /* || !builder.IntegratedSecurity */)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.Username) /* || !builder.IntegratedSecurity */)
        {
            builder.Username = HiddenValueDefault;
        }

        return builder.ToString();
    }

    public bool TryValidateConnectionString(string connectionString)
    {
        _ = new NpgsqlConnectionStringBuilder(connectionString);
        return true;
    }
}
