namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Event arguments for connection events
/// </summary>
public class ConnectionEventArgs : EventArgs
{
    /// <summary>
    /// Connection state when the event occurred
    /// </summary>
    public ConnectionState State { get; }
    
    /// <summary>
    /// Connection identifier
    /// </summary>
    public string ConnectionId { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Error message (if applicable)
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Exception details (if applicable)
    /// </summary>
    public Exception? Exception { get; }
    
    /// <summary>
    /// Additional context information
    /// </summary>
    public Dictionary<string, object>? Context { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConnectionEventArgs class
    /// </summary>
    /// <param name="state">Connection state</param>
    /// <param name="connectionId">Connection identifier</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    /// <param name="context">Additional context information</param>
    public ConnectionEventArgs(
        ConnectionState state, 
        string connectionId, 
        string? errorMessage = null, 
        Exception? exception = null, 
        Dictionary<string, object>? context = null)
    {
        State = state;
        ConnectionId = connectionId;
        Timestamp = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        Exception = exception;
        Context = context;
    }
}

/// <summary>
/// Event arguments for connection state changes
/// </summary>
public class ConnectionStateChangedEventArgs : ConnectionEventArgs
{
    /// <summary>
    /// Previous connection state
    /// </summary>
    public ConnectionState PreviousState { get; }
    
    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState CurrentState { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConnectionStateChangedEventArgs class
    /// </summary>
    /// <param name="previousState">Previous connection state</param>
    /// <param name="currentState">Current connection state</param>
    /// <param name="connectionId">Connection identifier</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    /// <param name="context">Additional context information</param>
    public ConnectionStateChangedEventArgs(
        ConnectionState previousState, 
        ConnectionState currentState, 
        string connectionId, 
        string? errorMessage = null, 
        Exception? exception = null, 
        Dictionary<string, object>? context = null)
        : base(currentState, connectionId, errorMessage, exception, context)
    {
        PreviousState = previousState;
        CurrentState = currentState;
    }
} 