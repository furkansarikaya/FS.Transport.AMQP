namespace FS.Transport.AMQP.Connection;

/// <summary>
/// Connection statistics and metrics
/// </summary>
public class ConnectionStatistics
{
    /// <summary>
    /// Total number of successful connections
    /// </summary>
    public long TotalConnections { get; set; }
    
    /// <summary>
    /// Total number of connection failures
    /// </summary>
    public long FailedConnections { get; set; }
    
    /// <summary>
    /// Total number of recovery attempts
    /// </summary>
    public long RecoveryAttempts { get; set; }
    
    /// <summary>
    /// Total number of successful recoveries
    /// </summary>
    public long SuccessfulRecoveries { get; set; }
    
    /// <summary>
    /// Current number of active channels
    /// </summary>
    public int ActiveChannels { get; set; }
    
    /// <summary>
    /// Current number of pooled channels
    /// </summary>
    public int PooledChannels { get; set; }
    
    /// <summary>
    /// Last connection time
    /// </summary>
    public DateTime? LastConnected { get; set; }
    
    /// <summary>
    /// Last disconnection time
    /// </summary>
    public DateTime? LastDisconnected { get; set; }
    
    /// <summary>
    /// Current connection uptime
    /// </summary>
    public TimeSpan? Uptime => LastConnected.HasValue && LastDisconnected?.CompareTo(LastConnected) < 0 
        ? DateTime.UtcNow - LastConnected.Value 
        : null;
    
    /// <summary>
    /// Connection availability percentage (last 24 hours)
    /// </summary>
    public double AvailabilityPercentage { get; set; } = 100.0;
}