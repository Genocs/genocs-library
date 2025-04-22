using System.ComponentModel.DataAnnotations;

namespace Genocs.Persistence.EFCore;

/// <summary>
/// The DatabaseSettings class is used to configure the database connection settings.
/// TODO: Rename to *Options and Move to *.Configuration namespace.
/// </summary>
public class DatabaseSettings : IValidatableObject
{
    /// <summary>
    /// The database provider to use (e.g., SqlServer, MySql, PostgreSql).
    /// </summary>
    public string DBProvider { get; set; } = string.Empty;

    /// <summary>
    /// The connection string to the database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(DBProvider))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(DBProvider)} is not configured",
                new[] { nameof(DBProvider) });
        }

        if (string.IsNullOrEmpty(ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(ConnectionString)} is not configured",
                new[] { nameof(ConnectionString) });
        }
    }
}