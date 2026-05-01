using System.ComponentModel.DataAnnotations;

namespace Genocs.Persistence.EFCore.Configurations;

public class DatabaseOptions : IValidatableObject
{
    public string DBProvider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// When true, pending EF Core migrations are applied automatically on startup.
    /// When false, the presence of unapplied migrations is treated as a fatal error
    /// and the host is stopped — use this to prevent unintended schema changes in
    /// production environments where migrations must be applied through a controlled pipeline.
    /// Defaults to false.
    /// </summary>
    public bool AutoApplyMigrations { get; set; } = false;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(DBProvider))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseOptions)}.{nameof(DBProvider)} is not configured",
                new[] { nameof(DBProvider) });
        }

        if (string.IsNullOrEmpty(ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseOptions)}.{nameof(ConnectionString)} is not configured",
                new[] { nameof(ConnectionString) });
        }
    }
}