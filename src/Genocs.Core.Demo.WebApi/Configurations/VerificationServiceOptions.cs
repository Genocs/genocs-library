namespace Genocs.Core.Demo.WebApi.Configurations;

public class VerificationServiceOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "verificationService";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = default!;
}
