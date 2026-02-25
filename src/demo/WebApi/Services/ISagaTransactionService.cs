using Genocs.Saga;

namespace Genocs.Library.Demo.WebApi.Services;

/// <summary>
/// Service for managing Saga transactions.
/// </summary>
public interface ISagaTransactionService
{
    /// <summary>
    /// Starts a new Saga transaction.
    /// </summary>
    /// <param name="text">The text associated with the transaction.</param>
    /// <param name="originator">The originator of the transaction.</param>
    /// <returns>The SagaId of the newly started transaction.</returns>
    Task<SagaId> StartTransactionAsync(string text, string originator);

    /// <summary>
    /// Completes an existing Saga transaction.
    /// </summary>
    /// <param name="sagaId">The ID of the Saga transaction to complete.</param>
    /// <param name="text">The text associated with the transaction.</param>
    /// <param name="originator">The originator of the transaction.</param>
    /// <returns>The SagaId of the completed transaction.</returns>
    Task<SagaId> CompleteTransactionAsync(string sagaId, string text, string originator);
}