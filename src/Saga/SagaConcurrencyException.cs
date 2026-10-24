namespace Genocs.Saga;

public sealed class SagaConcurrencyException : SagaException
{
    public SagaConcurrencyException(string message)
        : base(message)
    {
    }

    public SagaConcurrencyException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}