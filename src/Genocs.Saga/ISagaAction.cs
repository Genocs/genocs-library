namespace Genocs.Saga;

public interface ISagaAction<in TMessage>
{
    Task HandleAsync(TMessage message, ISagaContext context);
    Task CompensateAsync(TMessage message, ISagaContext context);
}
