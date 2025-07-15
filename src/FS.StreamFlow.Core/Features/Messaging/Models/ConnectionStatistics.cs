namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents connection statistics and metrics
/// </summary>
public class ConnectionStatistics
{
    /// <summary>
    /// Total number of connection attempts
    /// </summary>
    public long TotalConnectionAttempts { get; set; }
    
    /// <summary>
    /// Number of successful connections
    /// </summary>
    public long SuccessfulConnections { get; set; }
    
    /// <summary>
    /// Number of failed connection attempts
    /// </summary>
    public long FailedConnections { get; set; }
    
    /// <summary>
    /// Total number of disconnections
    /// </summary>
    public long TotalDisconnections { get; set; }
    
    /// <summary>
    /// Number of reconnection attempts
    /// </summary>
    public long ReconnectionAttempts { get; set; }
    
    /// <summary>
    /// Current connection uptime
    /// </summary>
    public TimeSpan CurrentUptime { get; set; }
    
    /// <summary>
    /// Total uptime across all connections
    /// </summary>
    public TimeSpan TotalUptime { get; set; }
    
    /// <summary>
    /// Average connection duration
    /// </summary>
    public TimeSpan AverageConnectionDuration { get; set; }
    
    /// <summary>
    /// Last connection timestamp
    /// </summary>
    public DateTimeOffset? LastConnectedAt { get; set; }
    
    /// <summary>
    /// Last disconnection timestamp
    /// </summary>
    public DateTimeOffset? LastDisconnectedAt { get; set; }
    
    /// <summary>
    /// Number of active channels
    /// </summary>
    public int ActiveChannels { get; set; }
    
    /// <summary>
    /// Total number of channels created
    /// </summary>
    public long TotalChannelsCreated { get; set; }
    
    /// <summary>
    /// Connection success rate (percentage)
    /// </summary>
    public double ConnectionSuccessRate => TotalConnectionAttempts > 0 
        ? (double)SuccessfulConnections / TotalConnectionAttempts * 100 
        : 0;
        
    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState CurrentState { get; set; }
    
    /// <summary>
    /// Last error message (if any)
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
} 