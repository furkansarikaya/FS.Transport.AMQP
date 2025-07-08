namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Exception thrown when transaction operations fail.
/// </summary>
/// <remarks>
/// Transaction exceptions occur when transactional messaging operations cannot
/// be completed due to resource constraints, timeout conditions, or broker
/// configuration issues. These exceptions help identify transaction-specific
/// problems that require specialized error handling.
/// </remarks>
[Serializable]
public sealed class TransactionException : MessagingException
{
    /// <summary>
    /// Gets the transaction identifier associated with the failed operation.
    /// </summary>
    /// <value>The transaction ID, or null if not available.</value>
    public string? TransactionId { get; }

    /// <summary>
    /// Gets the transaction state when the error occurred.
    /// </summary>
    /// <value>The transaction state at the time of the error.</value>
    public TransactionState? TransactionState { get; }

    /// <summary>
    /// Gets the type of transaction operation that failed.
    /// </summary>
    /// <value>The operation type (e.g., "Commit", "Rollback", "Enlist"), or null if not specified.</value>
    public string? OperationType { get; }

    /// <summary>
    /// Initializes a new instance of the TransactionException class.
    /// </summary>
    /// <param name="message">The error message describing the transaction failure.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="transactionState">The transaction state when the error occurred.</param>
    /// <param name="operationType">The type of operation that failed.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public TransactionException(string message, string? transactionId = null, TransactionState? transactionState = null, string? operationType = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, errorCode, context)
    {
        TransactionId = transactionId;
        TransactionState = transactionState;
        OperationType = operationType;
    }

    /// <summary>
    /// Initializes a new instance of the TransactionException class.
    /// </summary>
    /// <param name="message">The error message describing the transaction failure.</param>
    /// <param name="innerException">The exception that caused the transaction failure.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="transactionState">The transaction state when the error occurred.</param>
    /// <param name="operationType">The type of operation that failed.</param>
    /// <param name="errorCode">Optional error code for programmatic error handling.</param>
    /// <param name="context">Optional additional context information.</param>
    public TransactionException(string message, Exception innerException, string? transactionId = null, TransactionState? transactionState = null, string? operationType = null, string? errorCode = null, IReadOnlyDictionary<string, object>? context = null)
        : base(message, innerException, errorCode, context)
    {
        TransactionId = transactionId;
        TransactionState = transactionState;
        OperationType = operationType;
    }

    /// <summary>
    /// Initializes a new instance of the TransactionException class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information.</param>
    private TransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        TransactionId = info.GetString(nameof(TransactionId));
        OperationType = info.GetString(nameof(OperationType));
        // Note: Enum serialization would need special handling
    }
}