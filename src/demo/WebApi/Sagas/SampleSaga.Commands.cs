using Genocs.Common.CQRS.Events;
using Genocs.Saga;

namespace Genocs.Library.Demo.WebApi.Sagas;

public sealed class StartSagaCommand
{
    public string? Text { get; set; }
    public int TransactionValue { get; set; }
}

public class StartTransaction : ISagaMessageIdentity
{
    public string? Text { get; set; }
    public int TransactionValue { get; set; }

    public string MessageId => Guid.NewGuid().ToString();
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
