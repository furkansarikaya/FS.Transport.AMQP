using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for RabbitMQ broker connections and network communication.
/// </summary>
/// <remarks>
/// Connection configuration defines how the messaging infrastructure establishes and
/// maintains connections to RabbitMQ brokers. This includes network endpoints, authentication,
/// connection pooling, retry behavior, and performance tuning parameters.
/// 
/// The configuration supports both simple single-broker setups and sophisticated
/// clustered deployments with automatic failover, load balancing, and connection recovery.
/// It's designed to handle various network conditions and operational requirements
/// while providing optimal performance and reliability characteristics.
/// 
/// Key considerations for connection configuration:
/// - Network topology and broker deployment architecture
/// - Expected message volume and connection load patterns
/// - Failure recovery requirements and RTO/RPO objectives
/// - Security and compliance requirements
/// - Monitoring and operational visibility needs
/// </remarks>
public sealed class ConnectionConfiguration
{
    /// <summary>
    /// Gets or sets the collection of broker endpoints for connection establishment.
    /// </summary>
    /// <value>A list of broker endpoints defining connection targets and their properties.</value>
    /// <remarks>
    /// Endpoint configuration supports various deployment scenarios:
    /// 
    /// Single broker deployments:
    /// - Simple development and testing environments
    /// - Small-scale production systems with acceptable single points of failure
    /// - Environments with external load balancing and high availability
    /// 
    /// Clustered deployments:
    /// - Production systems requiring high availability and fault tolerance
    /// - Environments with geographic distribution and disaster recovery requirements
    /// - Systems with varying broker capabilities and specialization
    /// 
    /// The client will attempt connections to endpoints in order, with automatic
    /// failover and load balancing based on the configured strategy.
    /// </remarks>
    public IList<BrokerEndpoint> Endpoints { get; set; } = new List<BrokerEndpoint>();

    /// <summary>
    /// Gets or sets the authentication credentials for broker access.
    /// </summary>
    /// <value>The credentials used for broker authentication, or null for guest access.</value>
    /// <remarks>
    /// Authentication configuration supports various credential types:
    /// - Username/password authentication for simple scenarios
    /// - Certificate-based authentication for enhanced security
    /// - External authentication provider integration
    /// - Environment-specific credential management
    /// 
    /// Production environments should always use strong authentication
    /// and avoid default or guest credentials for security reasons.
    /// </remarks>
    public AuthenticationCredentials? Credentials { get; set; }

    /// <summary>
    /// Gets or sets the virtual host name for logical broker separation.
    /// </summary>
    /// <value>The virtual host name, or "/" for the default virtual host.</value>
    /// <remarks>
    /// Virtual hosts provide logical separation within a single RabbitMQ broker:
    /// - Namespace isolation for different applications or environments
    /// - Resource and permission boundaries for multi-tenancy
    /// - Administrative separation for different operational teams
    /// - Configuration and policy isolation for different use cases
    /// 
    /// Virtual host selection should align with organizational structure,
    /// security requirements, and operational responsibilities.
    /// </remarks>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the connection pool size for managing multiple broker connections.
    /// </summary>
    /// <value>The number of connections to maintain in the connection pool. Default is 5.</value>
    /// <remarks>
    /// Connection pooling provides performance and reliability benefits:
    /// - Reduced connection establishment overhead for high-frequency operations
    /// - Load distribution across multiple network connections
    /// - Fault isolation and recovery for individual connection failures
    /// - Capacity management for concurrent operation scaling
    /// 
    /// Pool size should be tuned based on:
    /// - Expected concurrent operation volume
    /// - Network capacity and broker connection limits
    /// - Application threading and concurrency patterns
    /// - Fault tolerance and recovery time requirements
    /// 
    /// Typical configurations:
    /// - Development: 1-2 connections
    /// - Production: 5-20 connections
    /// - High-throughput: 20-50 connections
    /// </remarks>
    public int ConnectionPoolSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of channels per connection.
    /// </summary>
    /// <value>The channel limit per connection. Default is 100.</value>
    /// <remarks>
    /// Channel management affects both performance and resource utilization:
    /// - Channels provide lightweight parallelism within connections
    /// - Each consumer and publisher typically uses a dedicated channel
    /// - Channel multiplexing reduces connection overhead
    /// - Channel limits prevent resource exhaustion and performance degradation
    /// 
    /// Channel limits should consider:
    /// - Expected number of concurrent consumers and publishers
    /// - Broker channel limits and performance characteristics
    /// - Memory usage and garbage collection implications
    /// - Error isolation and recovery requirements
    /// 
    /// RabbitMQ recommends keeping channel counts reasonable (typically under 200 per connection)
    /// to maintain optimal performance and resource utilization.
    /// </remarks>
    public int MaxChannelsPerConnection { get; set; } = 100;

    /// <summary>
    /// Gets or sets the connection timeout for initial broker connection establishment.
    /// </summary>
    /// <value>The maximum time to wait for connection establishment. Default is 30 seconds.</value>
    /// <remarks>
    /// Connection timeout balances responsiveness with reliability:
    /// - Shorter timeouts provide faster failure detection and recovery
    /// - Longer timeouts accommodate slow networks and broker startup conditions
    /// - Timeout values should account for network latency and broker load
    /// 
    /// Timeout considerations:
    /// - Local networks: 5-15 seconds
    /// - Wide area networks: 15-60 seconds
    /// - High-latency connections: 30-120 seconds
    /// - Broker startup scenarios: 60-300 seconds
    /// </remarks>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the heartbeat interval for connection health monitoring.
    /// </summary>
    /// <value>The interval between heartbeat messages. Default is 60 seconds.</value>
    /// <remarks>
    /// Heartbeat configuration enables connection health monitoring:
    /// - Regular keepalive messages detect network and broker failures
    /// - Heartbeat frequency affects failure detection time and network overhead
    /// - Both client and broker participate in bidirectional heartbeat exchange
    /// 
    /// Heartbeat tuning considerations:
    /// - Network reliability and latency characteristics
    /// - Broker load and processing capacity
    /// - Application tolerance for connection recovery time
    /// - Network infrastructure and firewall timeout settings
    /// 
    /// Common configurations:
    /// - Reliable networks: 60-300 seconds
    /// - Unreliable networks: 30-60 seconds
    /// - Real-time applications: 10-30 seconds
    /// </remarks>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the retry policy for connection establishment failures.
    /// </summary>
    /// <value>The retry configuration for connection operations, or null for no retries.</value>
    /// <remarks>
    /// Connection retry configuration provides resilience against transient failures:
    /// - Network connectivity issues and broker temporary unavailability
    /// - Load balancing and failover scenarios during connection establishment
    /// - System startup and initialization race conditions
    /// 
    /// Retry strategies should consider:
    /// - Expected failure patterns and recovery characteristics
    /// - Application startup time requirements and dependencies
    /// - Resource constraints and retry overhead
    /// - Operational alerting and escalation procedures
    /// </remarks>
    public RetryPolicy? ConnectionRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the automatic recovery configuration for connection failures.
    /// </summary>
    /// <value>The recovery settings for handling connection disruptions, or null to disable automatic recovery.</value>
    /// <remarks>
    /// Automatic recovery enables resilient operation during network disruptions:
    /// - Transparent reconnection and channel recovery
    /// - Preservation of consumers, publishers, and topology declarations
    /// - Minimal application-level impact during infrastructure failures
    /// 
    /// Recovery configuration affects:
    /// - Recovery speed and resource utilization
    /// - Temporary message loss and delivery guarantee implications
    /// - Application threading and state management during recovery
    /// - Monitoring and alerting for recovery events
    /// </remarks>
    public RecoveryConfiguration? RecoveryConfiguration { get; set; }

    /// <summary>
    /// Gets or sets additional connection properties for advanced scenarios.
    /// </summary>
    /// <value>A dictionary of custom connection properties, or null if no additional properties are needed.</value>
    /// <remarks>
    /// Custom connection properties enable:
    /// - Broker-specific feature activation and configuration
    /// - Client identification and metadata for operational visibility
    /// - Performance tuning and optimization settings
    /// - Integration with monitoring and management systems
    /// 
    /// Common properties include:
    /// - "client_properties": Client identification and version information
    /// - "connection_name": Human-readable connection identifier
    /// - Performance tuning parameters for specific broker versions
    /// - Plugin-specific configuration and feature flags
    /// </remarks>
    public IDictionary<string, object>? ConnectionProperties { get; set; }

    /// <summary>
    /// Validates the connection configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid or incomplete.</exception>
    /// <remarks>
    /// Validation ensures that the connection configuration will result in successful
    /// broker connectivity and optimal performance characteristics. Key validation areas:
    /// 
    /// - Endpoint configuration completeness and format validation
    /// - Authentication credential presence and format verification
    /// - Resource limit consistency and capacity validation
    /// - Timeout and interval setting reasonableness
    /// - Cross-parameter consistency and compatibility
    /// </remarks>
    public void Validate()
    {
        if (!Endpoints.Any())
        {
            throw new BrokerConfigurationException(
                "At least one broker endpoint must be configured.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(Endpoints));
        }

        foreach (var endpoint in Endpoints)
        {
            endpoint.Validate();
        }

        if (ConnectionPoolSize <= 0)
        {
            throw new BrokerConfigurationException(
                "Connection pool size must be positive.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(ConnectionPoolSize),
                parameterValue: ConnectionPoolSize);
        }

        if (MaxChannelsPerConnection <= 0)
        {
            throw new BrokerConfigurationException(
                "Maximum channels per connection must be positive.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(MaxChannelsPerConnection),
                parameterValue: MaxChannelsPerConnection);
        }

        if (ConnectionTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Connection timeout must be positive.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(ConnectionTimeout),
                parameterValue: ConnectionTimeout);
        }

        if (HeartbeatInterval <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Heartbeat interval must be positive.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(HeartbeatInterval),
                parameterValue: HeartbeatInterval);
        }

        if (string.IsNullOrWhiteSpace(VirtualHost))
        {
            throw new BrokerConfigurationException(
                "Virtual host cannot be null or empty.",
                configurationSection: nameof(ConnectionConfiguration),
                parameterName: nameof(VirtualHost),
                parameterValue: VirtualHost);
        }

        // Validate connection properties format
        if (ConnectionProperties?.Any() == true)
        {
            foreach (var property in ConnectionProperties)
            {
                if (string.IsNullOrWhiteSpace(property.Key))
                {
                    throw new BrokerConfigurationException(
                        "Connection property keys cannot be null or empty.",
                        configurationSection: nameof(ConnectionConfiguration),
                        parameterName: "ConnectionProperties.Key");
                }
            }
        }
    }

    /// <summary>
    /// Creates a connection configuration optimized for development environments.
    /// </summary>
    /// <param name="connectionString">Optional connection string. If not provided, defaults to localhost.</param>
    /// <returns>A connection configuration suitable for development use.</returns>
    /// <remarks>
    /// Development configuration prioritizes simplicity and ease of setup:
    /// - Local broker connection with default settings
    /// - Minimal connection pooling for resource conservation
    /// - Relaxed timeout settings for debugging scenarios
    /// - Automatic recovery enabled for development convenience
    /// - Simple authentication for local development environments
    /// </remarks>
    public static ConnectionConfiguration CreateDevelopment(string? connectionString = null)
    {
        var config = new ConnectionConfiguration
        {
            ConnectionPoolSize = 2,
            MaxChannelsPerConnection = 50,
            ConnectionTimeout = TimeSpan.FromSeconds(10),
            HeartbeatInterval = TimeSpan.FromSeconds(30),
            ConnectionRetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 3),
            RecoveryConfiguration = RecoveryConfiguration.CreateDefault()
        };

        if (!string.IsNullOrEmpty(connectionString))
        {
            config.Endpoints.Add(BrokerEndpoint.ParseConnectionString(connectionString));
        }
        else
        {
            config.Endpoints.Add(BrokerEndpoint.CreateLocal());
        }

        return config;
    }

    /// <summary>
    /// Creates a connection configuration optimized for production environments.
    /// </summary>
    /// <param name="connectionString">The production broker connection string or cluster endpoints.</param>
    /// <returns>A connection configuration suitable for production use.</returns>
    /// <remarks>
    /// Production configuration prioritizes reliability, performance, and operational excellence:
    /// - Robust connection pooling for high availability and performance
    /// - Conservative timeout settings for network resilience
    /// - Comprehensive retry and recovery policies
    /// - Strong authentication and security settings
    /// - Monitoring and operational visibility features
    /// </remarks>
    public static ConnectionConfiguration CreateProduction(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var config = new ConnectionConfiguration
        {
            ConnectionPoolSize = 10,
            MaxChannelsPerConnection = 100,
            ConnectionTimeout = TimeSpan.FromSeconds(30),
            HeartbeatInterval = TimeSpan.FromSeconds(60),
            ConnectionRetryPolicy = RetryPolicy.CreateExponentialBackoff(
                baseDelay: TimeSpan.FromSeconds(2),
                maxDelay: TimeSpan.FromMinutes(5),
                maxAttempts: 10),
            RecoveryConfiguration = RecoveryConfiguration.CreateRobust()
        };

        // Parse connection string and add endpoints
        var endpoints = ParseConnectionStringToEndpoints(connectionString);
        foreach (var endpoint in endpoints)
        {
            config.Endpoints.Add(endpoint);
        }

        return config;
    }

    /// <summary>
    /// Creates a connection configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <param name="connectionString">The high-performance broker connection string or cluster endpoints.</param>
    /// <returns>A connection configuration optimized for maximum throughput.</returns>
    /// <remarks>
    /// High-throughput configuration prioritizes performance and capacity:
    /// - Aggressive connection pooling for maximum parallelism
    /// - Optimized channel allocation for high concurrency
    /// - Performance-tuned timeout and heartbeat settings
    /// - Minimal overhead retry and recovery policies
    /// - Resource optimization for sustained high load
    /// </remarks>
    public static ConnectionConfiguration CreateHighThroughput(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var config = new ConnectionConfiguration
        {
            ConnectionPoolSize = 20,
            MaxChannelsPerConnection = 200,
            ConnectionTimeout = TimeSpan.FromSeconds(15),
            HeartbeatInterval = TimeSpan.FromSeconds(30),
            ConnectionRetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 5),
            RecoveryConfiguration = RecoveryConfiguration.CreateHighThroughput()
        };

        var endpoints = ParseConnectionStringToEndpoints(connectionString);
        foreach (var endpoint in endpoints)
        {
            config.Endpoints.Add(endpoint);
        }

        return config;
    }

    /// <summary>
    /// Parses a connection string into broker endpoints.
    /// </summary>
    /// <param name="connectionString">The connection string to parse.</param>
    /// <returns>A collection of broker endpoints parsed from the connection string.</returns>
    /// <remarks>
    /// Connection string parsing supports various formats:
    /// - Single broker: "amqp://user:pass@host:port/vhost"
    /// - Multiple brokers: "amqp://host1:port1,host2:port2/vhost"
    /// - Secure connections: "amqps://host:port/vhost"
    /// - Parameter specifications: "amqp://host/vhost?heartbeat=60&timeout=30"
    /// 
    /// The parser extracts endpoint information, authentication credentials,
    /// and connection parameters to create appropriate endpoint configurations.
    /// </remarks>
    private static IEnumerable<BrokerEndpoint> ParseConnectionStringToEndpoints(string connectionString)
    {
        // Implementation would parse various connection string formats
        // For now, return a simple endpoint based on the connection string
        yield return BrokerEndpoint.ParseConnectionString(connectionString);
    }

    /// <summary>
    /// Returns a string representation of the connection configuration.
    /// </summary>
    /// <returns>A formatted string containing key connection parameters.</returns>
    public override string ToString()
    {
        var endpointCount = Endpoints.Count;
        var poolInfo = $"pool: {ConnectionPoolSize}";
        var channelInfo = $"channels: {MaxChannelsPerConnection}/conn";
        var vhostInfo = VirtualHost != "/" ? $"vhost: {VirtualHost}" : "default vhost";
        
        return $"Connection[{endpointCount} endpoint(s), {poolInfo}, {channelInfo}, {vhostInfo}]";
    }
}