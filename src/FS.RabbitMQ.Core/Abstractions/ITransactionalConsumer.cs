namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines a transactional consumer that processes messages within transaction scopes.
/// </summary>
/// <typeparam name="T">The type of message payload being consumed.</typeparam>
/// <remarks>
/// Transactional consumers provide ACID properties for message processing by wrapping
/// message handling within transaction scopes. This ensures that message acknowledgment
/// and business logic execute atomically, providing strong consistency guarantees.
/// 
/// Key characteristics:
/// - Automatic transaction scope management for message processing
/// - All-or-nothing semantics for message handling and acknowledgment
/// - Integration with external transactional resources and systems
/// - Comprehensive rollback and recovery mechanisms for processing failures
/// </remarks>
public interface ITransactionalConsumer<T> : IMessageConsumer where T : class
{
    /// <summary>
    /// Gets the current transaction timeout setting.
    /// </summary>
    /// <value>The maximum duration allowed for message processing within a transaction scope.</value>
    /// <remarks>
    /// Transaction timeout prevents indefinite resource locking:
    /// - Ensures timely completion of transactional operations
    /// - Provides protection against hung or slow processing logic
    /// - Enables predictable resource utilization and capacity planning
    /// - Can be tuned based on processing requirements and SLA commitments
    /// 
    /// Transactions that exceed the timeout are automatically rolled back
    /// and the message is made available for reprocessing.
    /// </remarks>
    TimeSpan TransactionTimeout { get; }

    /// <summary>
    /// Updates the transaction timeout setting dynamically.
    /// </summary>
    /// <param name="newTimeout">The new transaction timeout to use. Must be positive.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous timeout update operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newTimeout"/> is not positive.</exception>
    /// <exception cref="ConsumerException">Thrown when the timeout cannot be updated due to consumer state issues.</exception>
    /// <remarks>
    /// Dynamic timeout updates enable:
    /// - Runtime adjustment based on observed processing characteristics
    /// - Response to changing system load and performance requirements
    /// - Implementation of adaptive timeout strategies
    /// - Testing and optimization of transaction processing parameters
    /// 
    /// Timeout changes take effect for subsequent message processing transactions
    /// and do not affect transactions currently in progress.
    /// </remarks>
    Task UpdateTransactionTimeoutAsync(
        TimeSpan newTimeout, 
        CancellationToken cancellationToken = default);
}