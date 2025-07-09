namespace FS.Transport.AMQP.Configuration;

/// <summary>
/// Connection settings for RabbitMQ server
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// RabbitMQ server hostname or IP address
    /// </summary>
    public string HostName { get; set; } = "localhost";
    
    /// <summary>
    /// RabbitMQ server port
    /// </summary>
    public int Port { get; set; } = 5672;
    
    /// <summary>
    /// Username for authentication
    /// </summary>
    public string UserName { get; set; } = "guest";
    
    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = "guest";
    
    /// <summary>
    /// Virtual host name
    /// </summary>
    public string VirtualHost { get; set; } = "/";
    
    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 30000; // 30 seconds
    
    /// <summary>
    /// Heartbeat interval in seconds (0 to disable)
    /// </summary>
    public ushort HeartbeatInterval { get; set; } = 60;
    
    /// <summary>
    /// Whether to use SSL/TLS connection
    /// </summary>
    public bool UseSsl { get; set; } = false;
    
    /// <summary>
    /// SSL/TLS settings (only used when UseSsl is true)
    /// </summary>
    public SslSettings Ssl { get; set; } = new();
    
    /// <summary>
    /// Maximum number of channels per connection
    /// </summary>
    public ushort RequestedChannelMax { get; set; } = 2047;
    
    /// <summary>
    /// Maximum frame size (0 for no limit)
    /// </summary>
    public uint RequestedFrameMax { get; set; } = 0;
    
    /// <summary>
    /// Client properties sent to server
    /// </summary>
    public Dictionary<string, object> ClientProperties { get; set; } = new();
    
    /// <summary>
    /// Connection name for identification
    /// </summary>
    public string? ConnectionName { get; set; }
    
    /// <summary>
    /// Maximum number of concurrent connections in pool
    /// </summary>
    public int MaxConnections { get; set; } = 10;
    
    /// <summary>
    /// Minimum number of connections to maintain in pool
    /// </summary>
    public int MinConnections { get; set; } = 1;
    
    /// <summary>
    /// Connection pool cleanup interval in milliseconds
    /// </summary>
    public int PoolCleanupIntervalMs { get; set; } = 60000; // 1 minute
    
    /// <summary>
    /// Whether to enable automatic recovery for connections
    /// </summary>
    public bool AutoRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Validates the connection settings and throws if invalid
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when settings are invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(HostName))
            throw new ArgumentException("HostName cannot be null or empty");
            
        if (Port <= 0 || Port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535");
            
        if (string.IsNullOrWhiteSpace(UserName))
            throw new ArgumentException("UserName cannot be null or empty");
            
        if (string.IsNullOrEmpty(Password))
            throw new ArgumentException("Password cannot be null or empty");
            
        if (string.IsNullOrWhiteSpace(VirtualHost))
            throw new ArgumentException("VirtualHost cannot be null or empty");
            
        if (ConnectionTimeoutMs <= 0)
            throw new ArgumentException("ConnectionTimeoutMs must be greater than 0");
            
        if (MaxConnections <= 0)
            throw new ArgumentException("MaxConnections must be greater than 0");
            
        if (MinConnections <= 0)
            throw new ArgumentException("MinConnections must be greater than 0");
            
        if (MinConnections > MaxConnections)
            throw new ArgumentException("MinConnections cannot be greater than MaxConnections");
            
        if (UseSsl)
        {
            Ssl.Validate();
        }
    }

    /// <summary>
    /// Creates a copy of the connection settings
    /// </summary>
    /// <returns>Cloned connection settings</returns>
    public ConnectionSettings Clone()
    {
        return new ConnectionSettings
        {
            HostName = HostName,
            Port = Port,
            UserName = UserName,
            Password = Password,
            VirtualHost = VirtualHost,
            ConnectionTimeoutMs = ConnectionTimeoutMs,
            HeartbeatInterval = HeartbeatInterval,
            UseSsl = UseSsl,
            Ssl = new SslSettings
            {
                ServerName = Ssl.ServerName,
                CertificatePath = Ssl.CertificatePath,
                CertificatePassword = Ssl.CertificatePassword,
                CheckCertificateRevocation = Ssl.CheckCertificateRevocation,
                AcceptInvalidCertificates = Ssl.AcceptInvalidCertificates,
                SslProtocol = Ssl.SslProtocol
            },
            RequestedChannelMax = RequestedChannelMax,
            RequestedFrameMax = RequestedFrameMax,
            ClientProperties = new Dictionary<string, object>(ClientProperties),
            ConnectionName = ConnectionName,
            MaxConnections = MaxConnections,
            MinConnections = MinConnections,
            PoolCleanupIntervalMs = PoolCleanupIntervalMs,
            AutoRecoveryEnabled = AutoRecoveryEnabled
        };
    }

    /// <summary>
    /// Gets a string representation of the connection settings (without sensitive data)
    /// </summary>
    /// <returns>Connection settings description</returns>
    public override string ToString()
    {
        return $"RabbitMQ Connection: {UserName}@{HostName}:{Port}{VirtualHost} " +
               $"(SSL: {UseSsl}, Heartbeat: {HeartbeatInterval}s, Pool: {MinConnections}-{MaxConnections})";
    }
}