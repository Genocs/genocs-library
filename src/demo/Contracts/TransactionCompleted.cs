using Genocs.Common.CQRS.Events;

namespace Genocs.Library.Demo.Contracts;

public sealed class TransactionCompleted : IEvent
{
    public string Text { get; init; }
    public int TransactionValue { get; init; }
}
