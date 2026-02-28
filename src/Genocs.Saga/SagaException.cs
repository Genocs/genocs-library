namespace Genocs.Saga;

public class SagaException : Exception
{
    public SagaException(string message)
        : base(message)
    {
    }

    public SagaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
