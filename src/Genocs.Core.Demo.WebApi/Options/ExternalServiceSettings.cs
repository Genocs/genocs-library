namespace Genocs.Core.Demo.WebApi.Options;

public class ExternalServiceSettings
{
    public const string Position = "ExternalService";

    public string Caller { get; set; } = default!;
    public string Private { get; set; } = default!;
    public string Public { get; set; } = default!;
}
