using Genocs.Common.CQRS.Commons;
using Genocs.Messaging;
using Genocs.Messaging.Outbox;
using Genocs.Saga;

namespace Genocs.Library.Demo.WebApi.Sagas;

public class SampleSaga(ILogger<SampleSaga> logger,
    IBusPublisher publisher,
    IMessageOutbox outbox) : Saga<SagaData>,
    ISagaStartAction<StartTransaction>,
    ISagaAction<CompleteTransaction>
{

    private readonly ILogger<SampleSaga> _logger = logger;
    private readonly IBusPublisher _publisher = publisher;
    private readonly IMessageOutbox _outbox = outbox;

    public Task HandleAsync(StartTransaction message, ISagaContext context)
    {
        _logger.LogInformation("StartTransaction reached!");

        Data.IsStartTransaction = true;
        Data.MessageId = message.MessageId;
        Data.TransactionValue = message.TransactionValue;

        CompleteSaga();
        return Task.CompletedTask;
    }

    public async Task HandleAsync(CompleteTransaction message, ISagaContext context)
    {
        _logger.LogInformation("CompleteTransaction reached!");

        if (Data.TransactionValue < 0)
        {
            throw new Exception("Simulated exception in CompleteTransaction");
        }

        await PublishEventAsync(message.ToEvent());

        Data.IsCompleteTransaction = true;
        CompleteSaga();
    }

    public Task CompensateAsync(StartTransaction message, ISagaContext context)
    {
        _logger.LogError($"{nameof(StartTransaction)} failed, compensating... {message}", message.Text);
        return Task.CompletedTask;
    }

    public Task CompensateAsync(CompleteTransaction message, ISagaContext context)
    {
        _logger.LogError($"{nameof(CompleteTransaction)} failed, compensating... {message}", message.Text);
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

    private async Task PublishEventAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        string spanContext = System.Diagnostics.Activity.Current?.Id;

        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(message, spanContext: spanContext, cancellationToken: cancellationToken);
            return;
        }

        await _publisher.PublishAsync(message, spanContext: spanContext, cancellationToken: cancellationToken);
    }
}
