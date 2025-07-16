namespace FS.StreamFlow.Core.Features.Messaging.Models;

/// <summary>
/// Represents the status of a messaging client
/// </summary>
public enum ClientStatus
{
    /// <summary>
    /// Client is not initialized
    /// </summary>
    NotInitialized,
    
    /// <summary>
    /// Client is initializing
    /// </summary>
    Initializing,
    
    /// <summary>
    /// Client is connecting
    /// </summary>
    Connecting,
    
    /// <summary>
    /// Client is connected and ready
    /// </summary>
    Connected,
    
    /// <summary>
    /// Client is disconnecting
    /// </summary>
    Disconnecting,
    
    /// <summary>
    /// Client is disconnected
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// Client is reconnecting
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// Client is shutting down
    /// </summary>
    ShuttingDown,
    
    /// <summary>
    /// Client is shut down
    /// </summary>
    ShutDown,
    
    /// <summary>
    /// Client is in an error state
    /// </summary>
    Error
}

/// <summary>
/// Event arguments for client status changes
/// </summary>
public class ClientStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous client status
    /// </summary>
    public ClientStatus PreviousStatus { get; }
    
    /// <summary>
    /// Current client status
    /// </summary>
    public ClientStatus CurrentStatus { get; }
    
    /// <summary>
    /// Client identifier
    /// </summary>
    public string ClientId { get; }
    
    /// <summary>
    /// Timestamp when the status changed
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
    /// Initializes a new instance of the ClientStatusChangedEventArgs class
    /// </summary>
    /// <param name="previousStatus">Previous client status</param>
    /// <param name="currentStatus">Current client status</param>
    /// <param name="clientId">Client identifier</param>
    /// <param name="errorMessage">Error message (if applicable)</param>
    /// <param name="exception">Exception details (if applicable)</param>
    /// <param name="context">Additional context information</param>
    public ClientStatusChangedEventArgs(
        ClientStatus previousStatus,
        ClientStatus currentStatus,
        string clientId,
        string? errorMessage = null,
        Exception? exception = null,
        Dictionary<string, object>? context = null)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        ClientId = clientId;
        Timestamp = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        Exception = exception;
        Context = context;
    }
}

/// <summary>
/// Represents client statistics
/// </summary>
public class ClientStatistics
{
    /// <summary>
    /// Client identifier
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current client status
    /// </summary>
    public ClientStatus Status { get; set; }
    
    /// <summary>
    /// Client start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Client uptime
    /// </summary>
    public TimeSpan Uptime => DateTimeOffset.UtcNow - StartTime;
    
    /// <summary>
    /// Total connections made
    /// </summary>
    public long TotalConnections { get; set; }
    
    /// <summary>
    /// Total reconnection attempts
    /// </summary>
    public long TotalReconnections { get; set; }
    
    /// <summary>
    /// Number of messages sent
    /// </summary>
    public long MessagesSent { get; set; }
    
    /// <summary>
    /// Number of messages received
    /// </summary>
    public long MessagesReceived { get; set; }
    
    /// <summary>
    /// Total bytes sent
    /// </summary>
    public long BytesSent { get; set; }
    
    /// <summary>
    /// Total bytes received
    /// </summary>
    public long BytesReceived { get; set; }
    
    /// <summary>
    /// Number of errors encountered
    /// </summary>
    public long ErrorCount { get; set; }
    
    /// <summary>
    /// Last error timestamp
    /// </summary>
    public DateTimeOffset? LastErrorAt { get; set; }
    
    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Connection statistics
    /// </summary>
    public ConnectionStatistics ConnectionStats { get; set; } = new();
    
    /// <summary>
    /// Producer statistics
    /// </summary>
    public ProducerStatistics ProducerStats { get; set; } = new();
    
    /// <summary>
    /// Consumer statistics
    /// </summary>
    public ConsumerStatistics ConsumerStats { get; set; } = new();
    
    /// <summary>
    /// Statistics collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents client configuration
/// </summary>
public class ClientConfiguration
{
    /// <summary>
    /// Client identifier
    /// </summary>
    public string ClientId { get; set; } = Guid.CreateVersion7().ToString();
    
    /// <summary>
    /// Client name
    /// </summary>
    public string ClientName { get; set; } = "StreamFlow Client";
    
    /// <summary>
    /// Connection settings
    /// </summary>
    public ConnectionSettings Connection { get; set; } = new();
    
    /// <summary>
    /// Producer settings
    /// </summary>
    public ProducerSettings Producer { get; set; } = new();
    
    /// <summary>
    /// Consumer settings
    /// </summary>
    public ConsumerSettings Consumer { get; set; } = new();
    
    /// <summary>
    /// Serialization settings
    /// </summary>
    public SerializationSettings Serialization { get; set; } = new();
    
    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
    
    /// <summary>
    /// Health check settings
    /// </summary>
    public HealthCheckSettings HealthCheck { get; set; } = new();
    
    /// <summary>
    /// Whether to enable automatic recovery
    /// </summary>
    public bool EnableAutoRecovery { get; set; } = true;
    
    /// <summary>
    /// Whether to enable heartbeat
    /// </summary>
    public bool EnableHeartbeat { get; set; } = true;
    
    /// <summary>
    /// Heartbeat interval
    /// </summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// Additional configuration properties
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
    
    /// <summary>
    /// Creates a new ClientConfiguration instance
    /// </summary>
    public ClientConfiguration()
    {
        Properties = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents connection settings
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// Connection string or host
    /// </summary>
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// Port number
    /// </summary>
    public int Port { get; set; } = 5672;
    
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = "guest";
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = "guest";
    
    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";
    
    /// <summary>
    /// Connection timeout
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Request timeout
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to use SSL
    /// </summary>
    public bool UseSsl { get; set; } = false;
    
    /// <summary>
    /// SSL settings
    /// </summary>
    public SslSettings? Ssl { get; set; }
    
    /// <summary>
    /// Additional connection properties
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
    
    /// <summary>
    /// Creates a new ConnectionSettings instance
    /// </summary>
    public ConnectionSettings()
    {
        Properties = new Dictionary<string, object>();
    }
}

/// <summary>
/// Represents SSL settings
/// </summary>
public class SslSettings
{
    /// <summary>
    /// Whether SSL is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// SSL certificate path
    /// </summary>
    public string? CertificatePath { get; set; }
    
    /// <summary>
    /// SSL certificate password
    /// </summary>
    public string? CertificatePassword { get; set; }
    
    /// <summary>
    /// Whether to verify SSL certificate
    /// </summary>
    public bool VerifyCertificate { get; set; } = true;
    
    /// <summary>
    /// SSL protocol version
    /// </summary>
    public string? ProtocolVersion { get; set; }
}

/// <summary>
/// Represents health check settings
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Health check interval
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Health check timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Number of failed health checks before marking as unhealthy
    /// </summary>
    public int FailureThreshold { get; set; } = 3;
    
    /// <summary>
    /// Whether to perform deep health checks
    /// </summary>
    public bool EnableDeepChecks { get; set; } = false;
} 