namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for operations that can participate in transactions.
/// </summary>
/// <remarks>
/// Transactional operations provide the building blocks for implementing
/// atomic message publishing within transaction scopes. Operations are
/// enlisted in transactions and executed as part of the commit process.
/// </remarks>
public interface ITransactionalOperation
{
    /// <summary>
    /// Gets the unique identifier for this operation.
    /// </summary>
    /// <value>A unique identifier for tracking and correlation purposes.</value>
    string OperationId { get; }

    /// <summary>
    /// Executes the operation as part of a transaction commit.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation execution.</returns>
    /// <exception cref="TransactionException">Thrown when the operation cannot be executed successfully.</exception>
    Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Compensates for the operation if transaction rollback is required.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous compensation operation.</returns>
    /// <remarks>
    /// Compensation is called during transaction rollback to undo any side effects
    /// that may have occurred during operation execution or preparation.
    /// </remarks>
    Task CompensateAsync(CancellationToken cancellationToken = default);
}