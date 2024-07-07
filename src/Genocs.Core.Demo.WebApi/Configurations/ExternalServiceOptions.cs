using System.ComponentModel.DataAnnotations;

namespace Genocs.Core.Demo.WebApi.Configurations;

/// <summary>
/// This class implements Options pattern with Validation.
/// </summary>
public class ExternalServiceOptions : IValidatableObject
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "externalService";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string Caller { get; set; } = default!;
    public string Private { get; set; } = default!;
    public string Public { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Caller))
        {
            yield return new ValidationResult("No Caller defined in ExternalService config", new[] { nameof(Caller) });
        }

        if (string.IsNullOrWhiteSpace(Private))
        {
            yield return new ValidationResult("No Private defined in ExternalService config", new[] { nameof(Private) });
        }

        if (string.IsNullOrWhiteSpace(Public))
        {
            yield return new ValidationResult("No Public defined in ExternalService config", new[] { nameof(Public) });
        }
    }
}
