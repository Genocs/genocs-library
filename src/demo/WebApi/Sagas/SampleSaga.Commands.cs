namespace Genocs.Library.Demo.WebApi.Sagas;

public sealed class StartSagaCommand
{
    public string? Text { get; set; }
    public int TransactionValue { get; set; }
}

public class StartTransaction
{
    public string? Text { get; set; }
    public int TransactionValue { get; set; }
}

public class CompleteTransaction
{
    public string? Text { get; set; }
    public int TransactionValue { get; set; }
}