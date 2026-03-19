using Genocs.Common.CQRS.Events;

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

public sealed class TransactionCompleted : IEvent
{
    public string? Text { get; init; }
    public int TransactionValue { get; init; }
}

public static class CommandsExtensions
{
    public static TransactionCompleted ToEvent(this CompleteTransaction command)
        => new() { Text = command.Text, TransactionValue = command.TransactionValue };
}
