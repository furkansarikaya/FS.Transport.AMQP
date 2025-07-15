namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Interface for provider-agnostic messaging channel operations
/// </summary>
public interface IChannel : IDisposable
{
    /// <summary>
    /// Gets the channel identifier
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Gets a value indicating whether the channel is open
    /// </summary>
    bool IsOpen { get; }
    
    /// <summary>
    /// Gets the channel state
    /// </summary>
    ChannelState State { get; }
    
    /// <summary>
    /// Gets the channel statistics
    /// </summary>
    ChannelStatistics Statistics { get; }
    
    /// <summary>
    /// Event raised when the channel state changes
    /// </summary>
    event EventHandler<ChannelStateChangedEventArgs>? StateChanged;
    
    /// <summary>
    /// Event raised when the channel is closed
    /// </summary>
    event EventHandler<ChannelEventArgs>? Closed;
    
    /// <summary>
    /// Closes the channel
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the close operation</returns>
    Task CloseAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a basic operation to test channel health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the channel is healthy, otherwise false</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the state of a messaging channel
/// </summary>
public enum ChannelState
{
    /// <summary>
    /// Channel is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Channel is being opened
    /// </summary>
    Opening,
    
    /// <summary>
    /// Channel is open and ready
    /// </summary>
    Open,
    
    /// <summary>
    /// Channel is being closed
    /// </summary>
    Closing,
    
    /// <summary>
    /// Channel is closed
    /// </summary>
    Closed,
    
    /// <summary>
    /// Channel is in an error state
    /// </summary>
    Error
}

/// <summary>
/// Represents channel statistics and metrics
/// </summary>
public class ChannelStatistics
{
    /// <summary>
    /// Channel identifier
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of messages sent
    /// </summary>
    public long MessagesSent { get; set; }
    
    /// <summary>
    /// Total number of messages received
    /// </summary>
    public long MessagesReceived { get; set; }
    
    /// <summary>
    /// Total number of bytes sent
    /// </summary>
    public long BytesSent { get; set; }
    
    /// <summary>
    /// Total number of bytes received
    /// </summary>
    public long BytesReceived { get; set; }
    
    /// <summary>
    /// Channel creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Number of errors encountered
    /// </summary>
    public long ErrorCount { get; set; }
    
    /// <summary>
    /// Current channel state
    /// </summary>
    public ChannelState CurrentState { get; set; }
}

/// <summary>
/// Event arguments for channel events
/// </summary>
public class ChannelEventArgs : EventArgs
{
    /// <summary>
    /// Channel identifier
    /// </summary>
    public string ChannelId { get; }
    
    /// <summary>
    /// Channel state when the event occurred
    /// </summary>
    public ChannelState State { get; }
    
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
    /// Initializes a new instance of the ChannelEventArgs class
    /// </summary>
    /// <param name="channelId">Channel identifier</param>
    /// <param name="state">Channel state</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    public ChannelEventArgs(
        string channelId, 
        ChannelState state, 
        string? errorMessage = null, 
        Exception? exception = null)
    {
        ChannelId = channelId;
        State = state;
        Timestamp = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
}

/// <summary>
/// Event arguments for channel state changes
/// </summary>
public class ChannelStateChangedEventArgs : ChannelEventArgs
{
    /// <summary>
    /// Previous channel state
    /// </summary>
    public ChannelState PreviousState { get; }
    
    /// <summary>
    /// Current channel state
    /// </summary>
    public ChannelState CurrentState { get; }
    
    /// <summary>
    /// Initializes a new instance of the ChannelStateChangedEventArgs class
    /// </summary>
    /// <param name="channelId">Channel identifier</param>
    /// <param name="previousState">Previous channel state</param>
    /// <param name="currentState">Current channel state</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    public ChannelStateChangedEventArgs(
        string channelId, 
        ChannelState previousState, 
        ChannelState currentState, 
        string? errorMessage = null, 
        Exception? exception = null)
        : base(channelId, currentState, errorMessage, exception)
    {
        PreviousState = previousState;
        CurrentState = currentState;
    }
} 