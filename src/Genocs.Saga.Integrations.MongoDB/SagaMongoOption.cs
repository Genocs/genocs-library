namespace Genocs.Saga.Integrations.MongoDB;

public sealed class SagaMongoOption
{
    public const string Position = "SagaMongo";
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
}
