namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the TransactionStateChanged event.
/// </summary>
/// <remarks>
/// State change events provide detailed information about transaction lifecycle
/// transitions, enabling comprehensive monitoring, logging, and error handling
/// for transactional messaging scenarios.
/// </remarks>
public sealed class TransactionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the transaction identifier for the state change.
    /// </summary>
    /// <value>The unique identifier of the transaction that changed state.</value>
    public string TransactionId { get; }

    /// <summary>
    /// Gets the previous state of the transaction.
    /// </summary>
    /// <value>The transaction state before the change occurred.</value>
    public TransactionState PreviousState { get; }

    /// <summary>
    /// Gets the current state of the transaction.
    /// </summary>
    /// <value>The transaction state after the change occurred.</value>
    public TransactionState CurrentState { get; }

    /// <summary>
    /// Gets the timestamp when the state change occurred.
    /// </summary>
    /// <value>The UTC timestamp of the state transition.</value>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional context information about the state change, if available.
    /// </summary>
    /// <value>A string containing details about the reason for the state change, or null if no additional information is available.</value>
    public string? Context { get; }

    /// <summary>
    /// Initializes a new instance of the TransactionStateChangedEventArgs class.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="previousState">The previous state of the transaction.</param>
    /// <param name="currentState">The current state of the transaction.</param>
    /// <param name="context">Optional additional context about the state change.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="transactionId"/> is null, empty, or whitespace.</exception>
    public TransactionStateChangedEventArgs(string transactionId, TransactionState previousState, TransactionState currentState, string? context = null)
    {
        TransactionId = !string.IsNullOrWhiteSpace(transactionId) 
            ? transactionId 
            : throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));
        PreviousState = previousState;
        CurrentState = currentState;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a value indicating whether the state change represents completion of the transaction.
    /// </summary>
    /// <value><c>true</c> if the transaction has reached a terminal state; otherwise, <c>false</c>.</value>
    public bool IsCompletion => CurrentState is TransactionState.Committed or TransactionState.RolledBack;

    /// <summary>
    /// Gets a value indicating whether the state change represents a successful outcome.
    /// </summary>
    /// <value><c>true</c> if the transaction was successfully committed; otherwise, <c>false</c>.</value>
    public bool IsSuccess => CurrentState == TransactionState.Committed;

    /// <summary>
    /// Returns a string representation of the transaction state change event.
    /// </summary>
    /// <returns>A formatted string describing the state transition.</returns>
    public override string ToString()
    {
        var contextInfo = !string.IsNullOrEmpty(Context) ? $" ({Context})" : "";
        return $"Transaction {TransactionId} changed from {PreviousState} to {CurrentState} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC{contextInfo}";
    }
}