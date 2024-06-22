namespace Genocs.Core.Demo.WebApi.Options;

public class SecretOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "secrets";


    /// <summary>
    /// This is an example of a secret. That should be stored in a secure way.
    /// </summary>
    public string? Secret { get; set; }

}
