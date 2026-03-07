namespace Genocs.Saga;

public class SagaContextError(Exception e)
{
    public Exception Exception { get; } = e;
}
