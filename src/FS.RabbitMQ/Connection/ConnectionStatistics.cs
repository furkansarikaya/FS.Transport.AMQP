namespace FS.RabbitMQ.Connection;

/// <summary>
/// Statistics for RabbitMQ connection operations
/// </summary>
public class ConnectionStatistics
{
    /// <summary>
    /// Gets or sets the total number of connection attempts
    /// </summary>
    public long ConnectionAttempts { get; set; }

    /// <summary>
    /// Gets or sets the number of successful connections
    /// </summary>
    public long ConnectionSuccesses { get; set; }

    /// <summary>
    /// Gets or sets the number of failed connection attempts
    /// </summary>
    public long ConnectionFailures { get; set; }

    /// <summary>
    /// Gets or sets the number of connection shutdowns
    /// </summary>
    public long ConnectionShutdowns { get; set; }

    /// <summary>
    /// Gets or sets the number of connection blocks
    /// </summary>
    public long ConnectionBlocks { get; set; }

    /// <summary>
    /// Gets or sets the number of connection unblocks
    /// </summary>
    public long ConnectionUnblocks { get; set; }

    /// <summary>
    /// Gets or sets the number of callback exceptions
    /// </summary>
    public long CallbackExceptions { get; set; }

    /// <summary>
    /// Gets or sets the number of channel shutdowns
    /// </summary>
    public long ChannelShutdowns { get; set; }

    /// <summary>
    /// Gets or sets the number of channel callback exceptions
    /// </summary>
    public long ChannelCallbackExceptions { get; set; }

    /// <summary>
    /// Gets or sets the number of recovery attempts
    /// </summary>
    public long RecoveryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the number of successful recoveries
    /// </summary>
    public long SuccessfulRecoveries { get; set; }

    /// <summary>
    /// Gets or sets the number of active channels
    /// </summary>
    public int ActiveChannels { get; set; }

    /// <summary>
    /// Gets or sets the number of health checks performed
    /// </summary>
    public long HealthChecks { get; set; }

    /// <summary>
    /// Gets or sets the number of failed health checks
    /// </summary>
    public long HealthCheckFailures { get; set; }

    /// <summary>
    /// Gets the connection success rate as a percentage
    /// </summary>
    public double SuccessRate => ConnectionAttempts > 0 ? 
        (double)ConnectionSuccesses / ConnectionAttempts * 100 : 0;

    /// <summary>
    /// Gets the connection failure rate as a percentage
    /// </summary>
    public double FailureRate => ConnectionAttempts > 0 ? 
        (double)ConnectionFailures / ConnectionAttempts * 100 : 0;

    /// <summary>
    /// Gets the recovery success rate as a percentage
    /// </summary>
    public double RecoverySuccessRate => RecoveryAttempts > 0 ? 
        (double)SuccessfulRecoveries / RecoveryAttempts * 100 : 0;

    /// <summary>
    /// Gets the health check success rate as a percentage
    /// </summary>
    public double HealthCheckSuccessRate => HealthChecks > 0 ? 
        (double)(HealthChecks - HealthCheckFailures) / HealthChecks * 100 : 0;

    /// <summary>
    /// Total number of connections (alias for backward compatibility)
    /// </summary>
    public long TotalConnections
    {
        get => ConnectionAttempts;
        set => ConnectionAttempts = value;
    }

    /// <summary>
    /// Number of failed connections (alias for backward compatibility)
    /// </summary>
    public long FailedConnections
    {
        get => ConnectionFailures;
        set => ConnectionFailures = value;
    }
}