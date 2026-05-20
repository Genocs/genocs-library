namespace Genocs.Library.Demo.WebApi.Sagas;

public class SagaData
{
    public bool IsStartTransaction { get; set; }
    public bool IsCompleteTransaction { get; set; }
    public int TransactionValue { get; set; }
    public bool IsEnded { get; set; }
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public bool IsSagaCompleted => IsStartTransaction && IsCompleteTransaction;
}
