using Genocs.Saga;

namespace Genocs.Library.Demo.Saga.WebApi.Sagas;

public class Message1
{
    public string? Text { get; set; }
}

public class Message2
{
    public string? Text { get; set; }
}

public class SagaData
{
    public bool IsMessage1 { get; set; }
    public bool IsMessage2 { get; set; }
}

public class SampleSaga(ILogger<SampleSaga> logger) : Saga<SagaData>, ISagaStartAction<Message1>, ISagaAction<Message2>
{

    private readonly ILogger<SampleSaga> _logger = logger;

    public Task HandleAsync(Message2 message, ISagaContext context)
    {
        Data.IsMessage2 = true;
        _logger.LogInformation("M2 reached!");
        CompleteSaga();
        return Task.CompletedTask;
    }

    public Task HandleAsync(Message1 message, ISagaContext context)
    {
        Data.IsMessage1 = true;
        _logger.LogInformation("M1 reached!");
        CompleteSaga();
        return Task.CompletedTask;
    }

    public Task CompensateAsync(Message1 message, ISagaContext context)
    {
        _logger.LogError("M1 failed, compensating... {message}",  message.Text);

        return Task.CompletedTask;
    }

    public Task CompensateAsync(Message2 message, ISagaContext context)
    {
        _logger.LogError("M2 failed, compensating... {message}", message.Text);
        return Task.CompletedTask;
    }

    private void CompleteSaga()
    {
        if (Data.IsMessage1 && Data.IsMessage2)
        {
            Complete();
            _logger.LogInformation("Saga completed!");
        }
    }
}
