namespace FS.RabbitMQ.Connection;

/// <summary>
/// Configuration settings for RabbitMQ connections
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// Gets or sets the RabbitMQ server hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the RabbitMQ server port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the username for authentication
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password for authentication
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the requested heartbeat interval
    /// </summary>
    public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the connection timeout
    /// </summary>
    public TimeSpan RequestedConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the client-provided connection name
    /// </summary>
    public string? ClientProvidedName { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of channels
    /// </summary>
    public int? MaxChannels { get; set; } = 20;

    /// <summary>
    /// Gets or sets the automatic recovery settings
    /// </summary>
    public AutomaticRecoverySettings AutomaticRecovery { get; set; } = new();

    /// <summary>
    /// Gets or sets the SSL settings
    /// </summary>
    public SslSettings Ssl { get; set; } = new();

    /// <summary>
    /// Gets or sets the health check settings
    /// </summary>
    public HealthCheckSettings HealthCheck { get; set; } = new();
}

/// <summary>
/// Settings for automatic recovery
/// </summary>
public class AutomaticRecoverySettings
{
    /// <summary>
    /// Gets or sets whether automatic recovery is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the network recovery interval
    /// </summary>
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets whether topology recovery is enabled
    /// </summary>
    public bool TopologyRecoveryEnabled { get; set; } = true;
}

/// <summary>
/// Settings for SSL/TLS connections
/// </summary>
public class SslSettings
{
    /// <summary>
    /// Gets or sets whether SSL is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the server name for SSL
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the SSL version
    /// </summary>
    public System.Security.Authentication.SslProtocols Version { get; set; } = 
        System.Security.Authentication.SslProtocols.Tls12;

    /// <summary>
    /// Gets or sets the acceptable policy errors
    /// </summary>
    public System.Net.Security.SslPolicyErrors AcceptablePolicyErrors { get; set; } = 
        System.Net.Security.SslPolicyErrors.None;

    /// <summary>
    /// Gets or sets the certificate path
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the certificate password
    /// </summary>
    public string? CertificatePassword { get; set; }
}

/// <summary>
/// Settings for health checks
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Gets or sets whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the health check timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
} 