namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Defines the contract for managing transactional message publishing operations.
/// </summary>
/// <remarks>
/// Transaction scopes provide ACID (Atomicity, Consistency, Isolation, Durability) properties
/// for message publishing operations, ensuring that multiple messages can be published as a
/// single atomic unit. This is particularly important for scenarios requiring strict consistency
/// guarantees and reliable message delivery.
/// 
/// Key characteristics:
/// - Atomicity: All messages in the transaction are published or none are
/// - Consistency: Broker state remains consistent regardless of transaction outcome
/// - Isolation: Concurrent transactions do not interfere with each other
/// - Durability: Committed messages survive broker restarts (when using durable queues)
/// 
/// Transaction scopes are essential for:
/// - Financial systems requiring strict consistency
/// - Multi-step workflows where partial completion is problematic
/// - Saga pattern implementations requiring compensation logic
/// - Integration scenarios with external transactional systems
/// 
/// Note: Transactions have significant performance overhead compared to regular publishing
/// and should be used only when strict transactional semantics are required.
/// </remarks>
public interface ITransactionScope : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this transaction scope.
    /// </summary>
    /// <value>A unique identifier that can be used for logging, monitoring, and correlation purposes.</value>
    /// <remarks>
    /// Transaction IDs enable:
    /// - Correlation of related operations within the same transaction
    /// - Monitoring and auditing of transaction lifecycles
    /// - Debugging and troubleshooting of transaction-related issues
    /// - Integration with distributed tracing systems
    /// </remarks>
    string TransactionId { get; }

    /// <summary>
    /// Gets the current state of the transaction.
    /// </summary>
    /// <value>The current state indicating the transaction's lifecycle phase.</value>
    /// <remarks>
    /// Transaction state provides visibility into the transaction lifecycle:
    /// - Active: Transaction is open and accepting operations
    /// - Committed: Transaction has been successfully committed
    /// - RolledBack: Transaction has been rolled back due to error or explicit rollback
    /// - Faulted: Transaction is in an error state and cannot be used
    /// 
    /// Applications can use state information to implement appropriate logic
    /// for different transaction phases and error conditions.
    /// </remarks>
    TransactionState State { get; }

    /// <summary>
    /// Gets the timestamp when the transaction was started.
    /// </summary>
    /// <value>The UTC timestamp of transaction creation.</value>
    /// <remarks>
    /// Start timestamps enable:
    /// - Transaction duration monitoring and performance analysis
    /// - Timeout detection for long-running transactions
    /// - Audit trails with accurate timing information
    /// - Correlation with external system events and metrics
    /// </remarks>
    DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the timeout duration for the transaction.
    /// </summary>
    /// <value>The maximum duration the transaction can remain active before automatic rollback.</value>
    /// <remarks>
    /// Transaction timeouts provide:
    /// - Protection against hung or abandoned transactions
    /// - Resource management and cleanup automation
    /// - Predictable system behavior under load
    /// - Integration with monitoring and alerting systems
    /// 
    /// Transactions that exceed the timeout are automatically rolled back
    /// to prevent resource leaks and ensure system stability.
    /// </remarks>
    TimeSpan Timeout { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction has been completed (committed or rolled back).
    /// </summary>
    /// <value><c>true</c> if the transaction is complete; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Completion status helps applications:
    /// - Avoid operations on completed transactions
    /// - Implement proper transaction lifecycle management
    /// - Detect programming errors related to transaction usage
    /// - Optimize resource cleanup and disposal patterns
    /// </remarks>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction can still accept new operations.
    /// </summary>
    /// <value><c>true</c> if operations can be added to the transaction; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Active status indicates whether the transaction is in a state where:
    /// - New publishing operations can be added
    /// - The transaction has not timed out or faulted
    /// - Commit or rollback operations can be performed
    /// - Resources are properly allocated and available
    /// </remarks>
    bool IsActive { get; }

    /// <summary>
    /// Commits the transaction, making all published messages durable and visible to consumers.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during commit.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for committing (already completed, faulted, or timed out).
    /// </exception>
    /// <exception cref="TransactionException">
    /// Thrown when the commit operation fails due to broker errors or resource constraints.
    /// </exception>
    /// <remarks>
    /// Commit operation:
    /// - Makes all messages published within the transaction visible to consumers
    /// - Ensures durability guarantees according to queue and message configuration
    /// - Releases transaction resources and locks
    /// - Transitions transaction state to Committed
    /// 
    /// After successful commit:
    /// - All messages are guaranteed to be delivered according to their durability settings
    /// - The transaction cannot be rolled back
    /// - Transaction resources are released and cleaned up
    /// - No further operations can be performed on the transaction
    /// 
    /// Commit failures typically indicate:
    /// - Network connectivity issues with the broker
    /// - Broker resource exhaustion or configuration problems
    /// - Timeout during the commit process
    /// - Concurrent transaction conflicts
    /// </remarks>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction, discarding all published messages and restoring the previous state.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during rollback.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    /// <exception cref="TransactionException">
    /// Thrown when the rollback operation fails due to broker errors or resource constraints.
    /// </exception>
    /// <remarks>
    /// Rollback operation:
    /// - Discards all messages published within the transaction
    /// - Restores the broker state to what it was before the transaction started
    /// - Releases transaction resources and locks
    /// - Transitions transaction state to RolledBack
    /// 
    /// After rollback:
    /// - No messages from the transaction are visible to consumers
    /// - The broker state is restored to the pre-transaction state
    /// - Transaction resources are released and cleaned up
    /// - No further operations can be performed on the transaction
    /// 
    /// Rollback is typically used when:
    /// - Business logic determines the transaction should not complete
    /// - Error conditions require undoing the transaction effects
    /// - External system integration fails and requires compensation
    /// - Timeout or resource constraints prevent successful completion
    /// 
    /// Rollback operations are generally more reliable than commits and rarely fail
    /// unless there are severe infrastructure issues.
    /// </remarks>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enlists a message publishing operation in the current transaction.
    /// </summary>
    /// <param name="publishOperation">The publishing operation to include in the transaction.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publishOperation"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for enlisting operations.
    /// </exception>
    /// <remarks>
    /// Enlisting operations allows the transaction to manage their lifecycle:
    /// - Operations are executed as part of the transaction commit process
    /// - All enlisted operations succeed together or fail together (atomicity)
    /// - Operations are isolated from other concurrent transactions
    /// - Resources are managed efficiently across multiple operations
    /// 
    /// Enlisted operations:
    /// - Are not visible to consumers until transaction commit
    /// - Can be rolled back without side effects
    /// - Participate in the transaction's durability guarantees
    /// - Are subject to the transaction's timeout constraints
    /// 
    /// This method is typically called internally by the publishing infrastructure
    /// rather than directly by application code. Applications should use the
    /// transactional publishing methods on IMessagePublisher instead.
    /// </remarks>
    void EnlistOperation(ITransactionalOperation publishOperation);

    /// <summary>
    /// Event raised when the transaction state changes.
    /// </summary>
    /// <remarks>
    /// State change events enable:
    /// - Monitoring of transaction lifecycle for audit and compliance
    /// - Implementation of transaction-aware application logic
    /// - Integration with external monitoring and alerting systems
    /// - Debugging and troubleshooting of transaction-related issues
    /// 
    /// Common state transitions:
    /// - Active -> Committed: Successful transaction completion
    /// - Active -> RolledBack: Explicit or automatic transaction rollback
    /// - Active -> Faulted: Error condition preventing further operations
    /// - Any state -> RolledBack: Timeout-triggered automatic rollback
    /// </remarks>
    event EventHandler<TransactionStateChangedEventArgs> StateChanged;
}