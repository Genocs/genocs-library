using Genocs.Library.Demo.WebApi.Sagas;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Saga;

namespace Genocs.Library.Demo.Services;

/// <summary>
/// Helper service to manage saga transactions,
/// providing methods to start and complete transactions while logging relevant information about the saga's progress and outcome.
/// </summary>
public class SagaTransactionService : ISagaTransactionService
{
    private readonly ISagaCoordinator _sagaCoordinator;
    private readonly ILogger<SagaTransactionService> _logger;

    /// <summary>
    /// Initializes a new instance of the SagaTransactionService class, coordinating saga transactions and logging
    /// related activities.
    /// </summary>
    /// <param name="sagaCoordinator">The saga coordinator responsible for managing the lifecycle and state of saga transactions.</param>
    /// <param name="logger">The logger used to record information and errors related to saga transaction processing.</param>
    public SagaTransactionService(ISagaCoordinator sagaCoordinator, ILogger<SagaTransactionService> logger)
    {
        _sagaCoordinator = sagaCoordinator;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new transaction asynchronously using the specified text and originator identifier. 
    /// </summary>
    /// <remarks>This method creates a new saga context and processes the transaction using the saga
    /// coordinator. Ensure that both the text and originator parameters are valid and non-null before calling this
    /// method.</remarks>
    /// <param name="text">The text that provides context or details for the transaction.</param>
    /// <param name="originator">The identifier of the entity initiating the transaction. Used to track the source of the transaction.</param>
    /// <returns>The <see cref="SagaId"/> of the newly started transaction.</returns>
    public async Task<SagaId> StartTransactionAsync(string text, string originator)
    {
        var context = SagaContext
            .Create()
            .WithSagaId(SagaId.NewSagaId())
            .WithOriginator(originator)
            .WithMetadata("key", "lulz")
            .Build();

        await _sagaCoordinator.ProcessAsync(
            new StartTransaction { Text = text },
            context);

        return context.SagaId;
    }

    /// <summary>
    /// Completes a saga transaction asynchronously using the specified saga identifier, message text, and originator
    /// information.
    /// </summary>
    /// <remarks>The method processes the transaction asynchronously and logs the outcome, including both
    /// successful completion and rejection scenarios. Ensure that all parameters are provided and valid to avoid
    /// unexpected behavior.</remarks>
    /// <param name="sagaId">The unique identifier for the saga associated with the transaction. Cannot be null.</param>
    /// <param name="text">The message text describing the transaction to be completed. Cannot be null or empty.</param>
    /// <param name="originator">The originator of the transaction, indicating the source of the request. Cannot be null.</param>
    /// <returns>The <see cref="SagaId"/> that represents the completed transaction.</returns>
    public async Task<SagaId> CompleteTransactionAsync(string sagaId, string text, string originator)
    {
        var context = SagaContext
            .Create()
            .WithSagaId(sagaId)
            .WithOriginator(originator)
            .WithMetadata("key", "lulz")
            .Build();

        await _sagaCoordinator.ProcessAsync(
            new CompleteTransaction { Text = text },
            onCompleted: (m, ctx) =>
            {
                _logger.LogInformation(
                    "Saga completed successfully with message: {MessageText} and metadata: {Metadata}",
                    m.Text,
                    string.Join(", ", ctx.Metadata.Select(md => $"{md.Key}={md.Value}")));

                return Task.CompletedTask;
            },
            onRejected: (m, ctx) =>
            {
                _logger.LogError(
                    "Saga rejected with message: {MessageText} and metadata: {Metadata}",
                    m.Text,
                    string.Join(", ", ctx.Metadata.Select(md => $"{md.Key}={md.Value}")));

                return Task.CompletedTask;
            },
            context: context);

        return context.SagaId;
    }
}