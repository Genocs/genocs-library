namespace Genocs.Core.Demo.WebApi.Configurations;

public class SecretOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "secrets";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// This is an example of a secret. That should be stored in a secure way.
    /// </summary>
    public string? Secret { get; set; }

}
