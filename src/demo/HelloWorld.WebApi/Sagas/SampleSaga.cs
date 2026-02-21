using Genocs.Saga;

namespace Genocs.Library.Demo.HelloWorld.WebApi.Sagas;

public class StartTransaction
{
    public string? Text { get; set; }
}

public class CompleteTransaction
{
    public string? Text { get; set; }
}

public class SagaData
{
    public bool IsStartTransaction { get; set; }
    public bool IsCompleteTransaction { get; set; }

    public bool IsEnded { get; set; }

    public bool IsSagaCompleted => IsStartTransaction && IsCompleteTransaction;
}

public class SampleSaga(ILogger<SampleSaga> logger) : Saga<SagaData>,
    ISagaStartAction<StartTransaction>,
    ISagaAction<CompleteTransaction>
{

    private readonly ILogger<SampleSaga> _logger = logger;

    public Task HandleAsync(StartTransaction message, ISagaContext context)
    {
        Data.IsStartTransaction = true;
        _logger.LogInformation("StartTransaction reached!");
        CompleteSaga();
        return Task.CompletedTask;
    }

    public Task HandleAsync(CompleteTransaction message, ISagaContext context)
    {
        Data.IsCompleteTransaction = true;
        _logger.LogInformation("CompleteTransaction reached!");
        CompleteSaga();
        return Task.CompletedTask;
    }

    public Task CompensateAsync(StartTransaction message, ISagaContext context)
    {
        _logger.LogError("StartTransaction failed, compensating... {message}", message.Text);

        return Task.CompletedTask;
    }

    public Task CompensateAsync(CompleteTransaction message, ISagaContext context)
    {
        _logger.LogError("CompleteTransaction failed, compensating... {message}", message.Text);
        return Task.CompletedTask;
    }

    private void CompleteSaga()
    {
        if (State == SagaProcessState.Completed)
            return;

        if (Data.IsSagaCompleted)
        {
            Complete();
            _logger.LogInformation("Saga completed!");
        }
    }
}
