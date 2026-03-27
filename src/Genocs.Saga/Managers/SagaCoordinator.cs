using Genocs.Saga.Async;

namespace Genocs.Saga.Managers;

internal sealed class SagaCoordinator : ISagaCoordinator
{
    private readonly ISagaSeeker _seeker;
    private readonly ISagaInitializer _initializer;
    private readonly ISagaProcessor _processor;
    private readonly ISagaPostProcessor _postProcessor;
    private readonly ISagaCompensationManager _compensationManager;
    private static readonly KeyedLocker Locker = new();

    public SagaCoordinator(
        ISagaSeeker seeker,
        ISagaInitializer initializer,
        ISagaProcessor processor,
        ISagaPostProcessor postProcessor,
        ISagaCompensationManager compensationManager)
    {
        _seeker = seeker;
        _initializer = initializer;
        _processor = processor;
        _postProcessor = postProcessor;
        _compensationManager = compensationManager;
    }

    public Task ProcessAsync<TMessage>(TMessage message, ISagaContext? context = null)
        where TMessage : class
            => ProcessAsync(message: message, onCompleted: null, onRejected: null, context: context);

    public async Task ProcessAsync<TMessage>(
        TMessage message,
        Func<TMessage, ISagaContext, Task>? onCompleted = null,
        Func<TMessage, ISagaContext, Task>? onRejected = null,
        ISagaContext? context = null)
        where TMessage : class
    {
        string messageType = typeof(TMessage).Name;
        using var processActivity = SagaTelemetry.StartProcessActivity(context, messageType);

        var actions = _seeker.Seek<TMessage>()?.ToList();

        static Task EmptyHook(TMessage m, ISagaContext ctx) => Task.CompletedTask;

        onCompleted ??= EmptyHook;
        onRejected ??= EmptyHook;

        var sagaTasks = actions?
            .ConvertAll(action => ProcessAsync(message, action, onCompleted, onRejected, context));

        if (sagaTasks != null)
        {
            await Task.WhenAll(sagaTasks);
        }
    }

    public Task RetryCompensationAsync<TSaga>(SagaId sagaId, ISagaContext? context = null)
        where TSaga : class, ISaga
        => _compensationManager.RetryAsync<TSaga>(sagaId, context);

    private async Task ProcessAsync<TMessage>(
        TMessage message,
        ISagaAction<TMessage> action,
        Func<TMessage, ISagaContext, Task> onCompleted,
        Func<TMessage, ISagaContext, Task> onRejected,
        ISagaContext? context = null)
        where TMessage : class
    {
        ISagaContext executionContext = new SagaExecutionContext(context ?? SagaContext.Empty);
        var saga = (ISaga)action;
        var id = saga.ResolveId(message, executionContext);
        string sagaType = saga.GetType().Name;
        string messageType = typeof(TMessage).Name;

        using var executeActivity = SagaTelemetry.StartExecuteActivity(executionContext, id, sagaType, messageType);

        using (await Locker.LockAsync(id))
        {
            var (isInitialized, state) = await _initializer.TryInitializeAsync(saga, id, message);

            if (!isInitialized || state == null)
            {
                return;
            }

            await _processor.ProcessAsync(saga, message, state, executionContext);
            await _postProcessor.ProcessAsync(saga, message, executionContext, onCompleted, onRejected);

            SagaTelemetry.SetSagaOutcome(saga.State, executionContext.SagaContextError);
        }
    }
}
