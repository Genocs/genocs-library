using System.ComponentModel.DataAnnotations;

namespace Genocs.Core.Demo.WebApi.Options;

public class ExternalServiceSettings : IValidatableObject
{
    public const string Position = "ExternalService";

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
