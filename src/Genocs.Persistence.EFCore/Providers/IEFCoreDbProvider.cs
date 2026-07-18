using Genocs.Persistence.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Providers;

/// <summary>
/// Abstraction over a single EF Core database provider (SQL Server, PostgreSQL, ...).
/// Implementations encapsulate everything that is specific to one database engine:
/// DbContext configuration, connection string masking and connection string validation.
/// </summary>
public interface IEFCoreDbProvider
{
    /// <summary>
    /// The provider key used to select this provider from configuration
    /// (see <see cref="DatabaseOptions.DBProvider"/>), e.g. "mssql" or "postgresql".
    /// Matching is case-insensitive.
    /// </summary>
    string ProviderKey { get; }

    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> for this database engine.
    /// </summary>
    /// <param name="builder">The options builder to configure.</param>
    /// <param name="options">The database settings (connection string, database name, ...).</param>
    void Configure(DbContextOptionsBuilder builder, DatabaseOptions options);

    /// <summary>
    /// Returns a copy of the connection string with sensitive information (user id, password) masked.
    /// </summary>
    /// <param name="connectionString">The connection string to secure.</param>
    /// <returns>The secured connection string.</returns>
    string MakeSecureConnectionString(string connectionString);

    /// <summary>
    /// Validates the connection string for this database engine.
    /// Implementations may throw provider-specific exceptions for malformed values;
    /// callers are expected to handle them.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>True when the connection string is valid.</returns>
    bool TryValidateConnectionString(string connectionString);
}
