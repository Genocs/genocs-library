namespace Genocs.Core.Domain.ConnectionString;

/// <summary>
/// Interface for validating connection strings.
/// </summary>
public interface IConnectionStringValidator
{
    /// <summary>
    /// Validates the given connection string.
    /// </summary>
    /// <param name="connectionString">The Connection string.</param>
    /// <param name="dbProvider">The Database provider name.</param>
    /// <returns>True in case of validated otherwise False.</returns>
    bool TryValidate(string connectionString, string? dbProvider = null);
}