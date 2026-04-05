namespace Genocs.Common.Validation;

/// <summary>
/// Defines a validation contract for application messages without binding to a specific validation library.
/// </summary>
/// <typeparam name="T">The model type to validate.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The model instance to validate.</param>
    /// <param name="cancellationToken">A token used to cancel the validation operation.</param>
    /// <returns>A validation result that contains success state and failures.</returns>
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}