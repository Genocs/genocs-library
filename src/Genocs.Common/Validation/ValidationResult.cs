namespace Genocs.Common.Validation;

/// <summary>
/// Represents the outcome of validation for a request or command model.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a reusable successful validation result with no errors.
    /// </summary>
    public static ValidationResult Success { get; } = new([]);

    /// <summary>
    /// Gets all validation failures.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether validation completed without failures.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether validation failed.
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Initializes a new validation result.
    /// </summary>
    /// <param name="errors">The validation failures. An empty set indicates success.</param>
    public ValidationResult(IEnumerable<ValidationError> errors)
    {
        Errors = errors?.ToArray() ?? [];
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    /// <returns>A failed validation result instance.</returns>
    public static ValidationResult Failure(params ValidationError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new ValidationResult(errors);
    }
}