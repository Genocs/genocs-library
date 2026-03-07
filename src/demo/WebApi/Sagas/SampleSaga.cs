using Genocs.Saga;

namespace Genocs.Library.Demo.WebApi.Sagas;

public class SampleSaga(ILogger<SampleSaga> logger) : Saga<SagaData>,
    ISagaStartAction<StartTransaction>,
    ISagaAction<CompleteTransaction>
{

    private readonly ILogger<SampleSaga> _logger = logger;

    public Task HandleAsync(StartTransaction message, ISagaContext context)
    {
        Data.IsStartTransaction = true;
        _logger.LogInformation("StartTransaction reached!");

        Data.TransactionValue = message.TransactionValue;

        CompleteSaga();
        return Task.CompletedTask;
    }

    public async Task HandleAsync(CompleteTransaction message, ISagaContext context)
    {
        Data.IsCompleteTransaction = true;
        _logger.LogInformation("CompleteTransaction reached!");

        if (Data.TransactionValue < 0)
        {
            throw new Exception("Simulated exception in CompleteTransaction");
        }

        CompleteSaga();
        await Task.CompletedTask;
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
        {
            _logger.LogInformation("Saga already completed.");
            return;
        }

        if (Data.IsSagaCompleted)
        {
            Complete();
            _logger.LogInformation("Saga completed!");
        }
    }
}
