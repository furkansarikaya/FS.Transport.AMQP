namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Represents the state of a transaction scope.
/// </summary>
/// <remarks>
/// Transaction states follow a specific lifecycle pattern and help applications
/// understand what operations are valid at any given time. State transitions
/// are generally irreversible, moving toward completion or failure.
/// </remarks>
public enum TransactionState
{
    /// <summary>
    /// The transaction is active and can accept new operations.
    /// This is the initial state when a transaction is created.
    /// </summary>
    /// <remarks>
    /// In the Active state:
    /// - New publishing operations can be enlisted
    /// - Commit and rollback operations are available
    /// - Transaction resources are allocated and maintained
    /// - Timeout monitoring is active
    /// </remarks>
    Active = 0,

    /// <summary>
    /// The transaction has been successfully committed and all operations are durable.
    /// This is a terminal state - no further operations are possible.
    /// </summary>
    /// <remarks>
    /// In the Committed state:
    /// - All enlisted messages are visible to consumers
    /// - Durability guarantees are in effect
    /// - Transaction resources have been released
    /// - No further operations are allowed
    /// </remarks>
    Committed = 1,

    /// <summary>
    /// The transaction has been rolled back and all operations have been discarded.
    /// This is a terminal state - no further operations are possible.
    /// </summary>
    /// <remarks>
    /// In the RolledBack state:
    /// - No enlisted messages are visible to consumers
    /// - Broker state has been restored to pre-transaction state
    /// - Transaction resources have been released
    /// - No further operations are allowed
    /// </remarks>
    RolledBack = 2,

    /// <summary>
    /// The transaction is in an error state and cannot be used for further operations.
    /// This typically leads to automatic rollback.
    /// </summary>
    /// <remarks>
    /// In the Faulted state:
    /// - An unrecoverable error has occurred
    /// - No new operations can be enlisted
    /// - Only rollback operations may be possible
    /// - Transaction will typically be rolled back automatically
    /// 
    /// Common causes of faulted state:
    /// - Network connectivity failures during transaction operations
    /// - Broker errors or resource exhaustion
    /// - Protocol violations or incompatible operations
    /// - System resource constraints or failures
    /// </remarks>
    Faulted = 3
}