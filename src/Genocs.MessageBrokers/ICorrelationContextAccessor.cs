namespace Genocs.MessageBrokers;

public interface ICorrelationContextAccessor
{
    object CorrelationContext { get; set; }
}