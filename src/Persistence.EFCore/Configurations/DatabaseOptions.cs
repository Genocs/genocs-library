using System.ComponentModel.DataAnnotations;

namespace Genocs.Persistence.EFCore.Configurations;

public class DatabaseOptions : IValidatableObject
{
    public string DBProvider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional explicit MongoDB database name.
    /// When omitted, the database name is derived from the MongoDB connection string path.
    /// </summary>
    public string? DatabaseName { get; set; }

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

        if (DBProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(GetMongoDatabaseName()))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseOptions)}.{nameof(DatabaseName)} must be configured or derivable from {nameof(ConnectionString)} when DBProvider is mongodb",
                new[] { nameof(DatabaseName), nameof(ConnectionString) });
        }
    }

    public string? GetMongoDatabaseName()
    {
        if (!string.IsNullOrWhiteSpace(DatabaseName))
        {
            return DatabaseName.Trim();
        }

        if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out var uri))
        {
            return null;
        }

        if (!uri.Scheme.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string databaseName = uri.AbsolutePath.Trim('/');
        return string.IsNullOrWhiteSpace(databaseName) ? null : databaseName;
    }
}