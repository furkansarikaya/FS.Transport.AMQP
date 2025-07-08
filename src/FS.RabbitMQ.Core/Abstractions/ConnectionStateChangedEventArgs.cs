namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Provides data for the ConnectionStateChanged event.
/// </summary>
/// <remarks>
/// Connection state change events are critical for implementing robust messaging applications
/// that need to respond to network conditions, broker availability, and connection lifecycle events.
/// Applications can use this information to implement connection-aware logic, such as:
/// - Pausing message publishing during disconnection
/// - Implementing graceful degradation strategies
/// - Triggering reconnection workflows
/// - Updating application health indicators
/// </remarks>
public sealed class ConnectionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous connection state before the transition.
    /// </summary>
    /// <value>The connection state that was active before the current change.</value>
    /// <remarks>
    /// The previous state provides context for understanding the nature of the state transition:
    /// - Connected -> Disconnected: Indicates connection loss (network issue, broker shutdown)
    /// - Disconnected -> Connecting: Indicates initial connection attempt or reconnection start
    /// - Connecting -> Connected: Indicates successful connection establishment
    /// - Connected -> Reconnecting: Indicates automatic recovery from connection disruption
    /// 
    /// State transition patterns help applications implement appropriate response strategies
    /// for different types of connection events.
    /// </remarks>
    public ConnectionState PreviousState { get; }

    /// <summary>
    /// Gets the current connection state after the transition.
    /// </summary>
    /// <value>The connection state that is now active after the change.</value>
    /// <remarks>
    /// The current state determines what operations are possible and what application
    /// behavior should be active:
    /// - Connected: Normal operations can proceed
    /// - Connecting/Reconnecting: Operations may be queued or delayed
    /// - Disconnected: Operations should be paused or queued for retry
    /// - Faulted: Manual intervention may be required
    /// </remarks>
    public ConnectionState CurrentState { get; }

    /// <summary>
    /// Gets the timestamp when the state change occurred.
    /// </summary>
    /// <value>The UTC timestamp of the state transition.</value>
    /// <remarks>
    /// Timestamps enable applications to:
    /// - Track connection stability metrics
    /// - Implement time-based reconnection strategies
    /// - Log connection events with accurate timing
    /// - Calculate connection uptime and availability statistics
    /// </remarks>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets additional context information about the state change, if available.
    /// </summary>
    /// <value>A string containing details about the state change reason, or null if no additional information is available.</value>
    /// <remarks>
    /// Context information may include:
    /// - Network error descriptions for disconnection events
    /// - Broker response details for connection failures
    /// - Configuration issues preventing connection establishment
    /// - Recovery progress information during reconnection attempts
    /// 
    /// This information is primarily useful for logging, debugging, and providing
    /// detailed feedback to operators or monitoring systems.
    /// </remarks>
    public string? Context { get; }

    /// <summary>
    /// Initializes a new instance of the ConnectionStateChangedEventArgs class.
    /// </summary>
    /// <param name="previousState">The previous connection state.</param>
    /// <param name="currentState">The current connection state.</param>
    /// <param name="context">Optional additional context about the state change.</param>
    /// <remarks>
    /// The timestamp is automatically set to the current UTC time when the instance is created.
    /// This ensures accurate timing information for state transitions without requiring
    /// explicit timestamp management by callers.
    /// </remarks>
    public ConnectionStateChangedEventArgs(ConnectionState previousState, ConnectionState currentState, string? context = null)
    {
        PreviousState = previousState;
        CurrentState = currentState;
        Context = context;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a value indicating whether the connection state transition represents an improvement in connectivity.
    /// </summary>
    /// <value><c>true</c> if the new state represents better connectivity; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// State transitions are considered improvements when they move toward greater connectivity:
    /// - Disconnected/Faulted -> Connecting: Recovery attempt started
    /// - Connecting/Reconnecting -> Connected: Connection successfully established
    /// 
    /// This property helps applications distinguish between degradation and recovery events
    /// without needing to implement state comparison logic.
    /// </remarks>
    public bool IsImprovement =>
        (PreviousState is ConnectionState.Disconnected or ConnectionState.Faulted && CurrentState == ConnectionState.Connecting) ||
        (PreviousState is ConnectionState.Connecting or ConnectionState.Reconnecting && CurrentState == ConnectionState.Connected);

    /// <summary>
    /// Gets a value indicating whether the connection state transition represents a degradation in connectivity.
    /// </summary>
    /// <value><c>true</c> if the new state represents worse connectivity; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// State transitions are considered degradations when they move toward lesser connectivity:
    /// - Connected -> Disconnecting/Reconnecting: Connection loss detected
    /// - Any state -> Faulted: Unrecoverable error condition
    /// - Connecting -> Disconnected: Connection attempt failed
    /// 
    /// Degradation events typically require application response such as pausing operations,
    /// implementing fallback strategies, or triggering alerts.
    /// </remarks>
    public bool IsDegradation =>
        (PreviousState == ConnectionState.Connected && CurrentState is ConnectionState.Disconnecting or ConnectionState.Reconnecting) ||
        (CurrentState == ConnectionState.Faulted) ||
        (PreviousState == ConnectionState.Connecting && CurrentState == ConnectionState.Disconnected);

    /// <summary>
    /// Returns a string representation of the connection state change event.
    /// </summary>
    /// <returns>A formatted string describing the state transition with timestamp.</returns>
    public override string ToString()
    {
        var contextInfo = !string.IsNullOrEmpty(Context) ? $" ({Context})" : "";
        return $"Connection state changed from {PreviousState} to {CurrentState} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC{contextInfo}";
    }
}