namespace Genocs.Common.Domain.ConnectionString;

/// <summary>
/// Interface for securing connection strings.
/// </summary>
public interface IConnectionStringSecurer
{
    /// <summary>
    /// Secures the connection string by removing sensitive information.
    /// </summary>
    /// <param name="connectionString">The Connection string.</param>
    /// <param name="dbProvider">The Database provider name.</param>
    /// <returns>The secured connection string.</returns>
    string? MakeSecure(string? connectionString, string? dbProvider = null);
}