using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.EntityFrameworkCore;
#if !NET10_0_OR_GREATER
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

namespace Genocs.Persistence.EFCore.MySql;

/// <summary>
/// MySQL implementation of <see cref="IEFCoreDbProvider"/>.
/// On net10.0 this is a stub until Pomelo.EntityFrameworkCore.MySql supports EF Core 10.
/// </summary>
public sealed class MySqlDbProvider : IEFCoreDbProvider
{
    public string ProviderKey => DbProviderKeys.MySql;

    public bool SupportsMigrations => true;

#if !NET10_0_OR_GREATER
    private const string HiddenValueDefault = "*******";

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseMySql(options.ConnectionString, ServerVersion.AutoDetect(options.ConnectionString), e =>
            e.MigrationsAssembly("Migrators.MySQL")
             .SchemaBehavior(MySqlSchemaBehavior.Ignore));

    public string MakeSecureConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

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
    {
        _ = new MySqlConnectionStringBuilder(connectionString);
        return true;
    }
#else
    // TODO: Re-enable when Pomelo.EntityFrameworkCore.MySql supports .NET 10.
    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => throw new NotSupportedException("MySQL is not yet supported on .NET 10. Awaiting Pomelo.EntityFrameworkCore.MySql update.");

    public string MakeSecureConnectionString(string connectionString)
        => connectionString;

    public bool TryValidateConnectionString(string connectionString)
        => true;
#endif
}
