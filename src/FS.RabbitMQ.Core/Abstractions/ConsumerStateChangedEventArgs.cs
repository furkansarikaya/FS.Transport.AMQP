namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the ConsumerStateChanged event.
/// </summary>
/// <remarks>
/// Consumer state change events provide detailed information about lifecycle
/// transitions, enabling comprehensive monitoring, logging, and automated
/// response to consumer operational changes.
/// </remarks>
public sealed class ConsumerStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the consumer identifier for the state change.
    /// </summary>
    /// <value>The unique identifier of the consumer that changed state.</value>
    public string ConsumerId { get; }

    /// <summary>
    /// Gets the previous state of the consumer.
    /// </summary>
    /// <value>The consumer state before the change occurred.</value>
    public ConsumerState PreviousState { get; }

    /// <summary>
    /// Gets the current state of the consumer.
    /// </summary>
    /// <value>The consumer state after the change occurred.</value>
    public ConsumerState CurrentState { get; }

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
    /// Initializes a new instance of the ConsumerStateChangedEventArgs class.
    /// </summary>
    /// <param name="consumerId">The unique identifier of the consumer.</param>
    /// <param name="previousState">The previous state of the consumer.</param>
    /// <param name="currentState">The current state of the consumer.</param>
    /// <param name="context">Optional additional context about the state change.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="consumerId"/> is null, empty, or whitespace.</exception>
    public ConsumerStateChangedEventArgs(string consumerId, ConsumerState previousState, ConsumerState currentState, string? context = null)
    {
        ConsumerId = !string.IsNullOrWhiteSpace(consumerId) 
            ? consumerId 
            : throw new ArgumentException("Consumer ID cannot be null or empty.", nameof(consumerId));
        PreviousState = previousState;
        CurrentState = currentState;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a value indicating whether the state change represents consumer activation.
    /// </summary>
    /// <value><c>true</c> if the consumer became active; otherwise, <c>false</c>.</value>
    public bool IsActivation => CurrentState == ConsumerState.Running && PreviousState != ConsumerState.Running;

    /// <summary>
    /// Gets a value indicating whether the state change represents consumer deactivation.
    /// </summary>
    /// <value><c>true</c> if the consumer became inactive; otherwise, <c>false</c>.</value>
    public bool IsDeactivation => PreviousState == ConsumerState.Running && CurrentState != ConsumerState.Running;

    /// <summary>
    /// Gets a value indicating whether the state change represents an error condition.
    /// </summary>
    /// <value><c>true</c> if the consumer entered a faulted state; otherwise, <c>false</c>.</value>
    public bool IsError => CurrentState == ConsumerState.Faulted;

    /// <summary>
    /// Returns a string representation of the consumer state change event.
    /// </summary>
    /// <returns>A formatted string describing the state transition.</returns>
    public override string ToString()
    {
        var contextInfo = !string.IsNullOrEmpty(Context) ? $" ({Context})" : "";
        return $"Consumer {ConsumerId} changed from {PreviousState} to {CurrentState} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC{contextInfo}";
    }
}
