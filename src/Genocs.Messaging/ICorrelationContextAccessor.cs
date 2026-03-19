namespace Genocs.Messaging;

public interface ICorrelationContextAccessor
{
    object? CorrelationContext { get; set; }
}