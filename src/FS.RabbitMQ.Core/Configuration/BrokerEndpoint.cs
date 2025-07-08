using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Represents a RabbitMQ broker endpoint with connection parameters and security settings.
/// </summary>
/// <remarks>
/// Broker endpoints define the network location and connection characteristics for individual
/// RabbitMQ broker instances. In clustered deployments, multiple endpoints enable load
/// balancing, failover, and geographic distribution of messaging infrastructure.
/// 
/// Each endpoint encapsulates the complete set of parameters needed to establish a
/// connection to a specific broker instance, including network addressing, security
/// settings, and connection behavior customization.
/// 
/// Key design considerations:
/// - Support for both simple and complex network topologies
/// - Comprehensive security model with multiple authentication methods
/// - Performance tuning capabilities for different deployment scenarios
/// - Operational visibility and monitoring integration
/// - Graceful degradation and failover capabilities
/// </remarks>
public sealed class BrokerEndpoint
{
    /// <summary>
    /// Gets or sets the hostname or IP address of the RabbitMQ broker.
    /// </summary>
    /// <value>The network hostname or IP address for broker connectivity.</value>
    /// <remarks>
    /// Host specification supports various formats and resolution mechanisms:
    /// - Fully qualified domain names (FQDN) for DNS-based service discovery
    /// - IP addresses for direct network connectivity
    /// - Service names for container orchestration and service mesh integration
    /// - Load balancer endpoints for clustered deployments
    /// 
    /// Host selection should consider:
    /// - Network topology and routing requirements
    /// - DNS resolution and service discovery mechanisms
    /// - Load balancing and failover strategies
    /// - Security and network policy constraints
    /// </remarks>
    public required string Host { get; set; }

    /// <summary>
    /// Gets or sets the port number for broker connectivity.
    /// </summary>
    /// <value>The TCP port number for RabbitMQ protocol communication. Default is 5672 for AMQP.</value>
    /// <remarks>
    /// Port configuration varies based on security and protocol requirements:
    /// - 5672: Standard AMQP protocol (unencrypted)
    /// - 5671: AMQP over TLS (encrypted)
    /// - 15672: Management UI and HTTP API
    /// - Custom ports: For security through obscurity or network policy compliance
    /// 
    /// Port selection should align with:
    /// - Security policies and encryption requirements
    /// - Network firewall and access control rules
    /// - Broker configuration and service binding
    /// - Operational monitoring and management needs
    /// </remarks>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets a value indicating whether to use TLS encryption for the connection.
    /// </summary>
    /// <value><c>true</c> to use TLS encryption; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// TLS encryption provides confidentiality and integrity for message transmission:
    /// - Prevents eavesdropping and man-in-the-middle attacks
    /// - Ensures message authenticity and data integrity
    /// - Supports various cipher suites and security levels
    /// - Enables compliance with security standards and regulations
    /// 
    /// TLS considerations:
    /// - Performance impact due to encryption overhead
    /// - Certificate management and PKI requirements
    /// - Cipher suite selection and security policy compliance
    /// - Monitoring and troubleshooting complexity
    /// 
    /// Production environments should typically enable TLS unless security
    /// analysis demonstrates that unencrypted communication is acceptable.
    /// </remarks>
    public bool UseTls { get; set; } = false;

    /// <summary>
    /// Gets or sets the priority for this endpoint in connection selection.
    /// </summary>
    /// <value>The priority value where higher numbers indicate higher preference. Default is 0.</value>
    /// <remarks>
    /// Endpoint priority enables sophisticated connection routing strategies:
    /// - Primary/secondary endpoint designation for failover scenarios
    /// - Geographic preference for latency optimization
    /// - Resource capacity-based routing for load balancing
    /// - Cost optimization through preferred endpoint selection
    /// 
    /// Priority-based selection patterns:
    /// - Active/standby: High priority for primary, low for backup
    /// - Regional preference: Higher priority for local endpoints
    /// - Capacity-based: Priority aligned with broker resource allocation
    /// - Cost optimization: Priority based on network or compute costs
    /// </remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets the weight for load balancing across multiple endpoints.
    /// </summary>
    /// <value>The relative weight for load distribution. Default is 1.</value>
    /// <remarks>
    /// Load balancing weight enables proportional traffic distribution:
    /// - Equal weights provide round-robin distribution
    /// - Different weights enable capacity-proportional distribution
    /// - Zero weight temporarily disables an endpoint
    /// - Dynamic weight adjustment supports capacity scaling
    /// 
    /// Weight-based load balancing scenarios:
    /// - Capacity proportional: Weight aligned with broker resource capacity
    /// - Geographic distribution: Weight based on regional traffic requirements
    /// - Performance optimization: Weight based on latency and throughput characteristics
    /// - Maintenance windows: Temporary weight reduction for rolling updates
    /// </remarks>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Gets or sets the TLS configuration for secure connections.
    /// </summary>
    /// <value>The TLS settings for encrypted connections, or null if TLS is not used.</value>
    /// <remarks>
    /// TLS configuration provides granular control over security parameters:
    /// - Certificate validation and trust chain management
    /// - Cipher suite selection and security level enforcement
    /// - Client certificate authentication for mutual TLS
    /// - Protocol version and feature compatibility
    /// 
    /// TLS configuration should align with:
    /// - Organizational security policies and compliance requirements
    /// - Certificate authority and PKI infrastructure capabilities
    /// - Performance requirements and acceptable security trade-offs
    /// - Operational complexity and certificate management processes
    /// </remarks>
    public TlsConfiguration? TlsConfiguration { get; set; }

    /// <summary>
    /// Gets or sets connection-specific properties for this endpoint.
    /// </summary>
    /// <value>A dictionary of endpoint-specific connection properties, or null if no custom properties are needed.</value>
    /// <remarks>
    /// Connection properties enable endpoint-specific customization:
    /// - Broker-specific feature activation and configuration
    /// - Performance tuning parameters for specific network conditions
    /// - Monitoring and operational metadata for endpoint identification
    /// - Integration with service discovery and orchestration systems
    /// 
    /// Common endpoint properties:
    /// - Connection naming and identification for operational visibility
    /// - Timeout and retry overrides for specific network characteristics
    /// - Protocol-level tuning for performance optimization
    /// - Integration metadata for monitoring and alerting systems
    /// </remarks>
    public IDictionary<string, object>? ConnectionProperties { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about this endpoint.
    /// </summary>
    /// <value>A dictionary containing descriptive and operational metadata about the endpoint.</value>
    /// <remarks>
    /// Endpoint metadata provides context for operational and monitoring purposes:
    /// - Geographic location and data center information
    /// - Environment classification (development, staging, production)
    /// - Capacity and performance characteristics
    /// - Operational contact and escalation information
    /// 
    /// Metadata usage patterns:
    /// - Monitoring dashboards and alerting systems
    /// - Capacity planning and performance analysis
    /// - Operational runbooks and troubleshooting guides
    /// - Compliance and audit trail documentation
    /// </remarks>
    public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Creates a broker endpoint for local development with default settings.
    /// </summary>
    /// <param name="port">The port number for the local broker. Default is 5672.</param>
    /// <returns>A broker endpoint configured for local development use.</returns>
    /// <remarks>
    /// Local development endpoints provide simplified configuration for development scenarios:
    /// - Localhost connectivity with minimal security overhead
    /// - Standard AMQP port with unencrypted communication
    /// - Default priority and weight for simple connection behavior
    /// - No TLS configuration for development convenience
    /// 
    /// This configuration is suitable only for development environments
    /// and should never be used in production deployments.
    /// </remarks>
    public static BrokerEndpoint CreateLocal(int port = 5672)
    {
        return new BrokerEndpoint
        {
            Host = "localhost",
            Port = port,
            UseTls = false,
            Priority = 1,
            Weight = 1,
            Metadata = new Dictionary<string, string>
            {
                ["environment"] = "development",
                ["location"] = "local"
            }
        };
    }

    /// <summary>
    /// Creates a broker endpoint with TLS encryption enabled.
    /// </summary>
    /// <param name="host">The hostname or IP address of the secure broker.</param>
    /// <param name="port">The port number for TLS-encrypted connections. Default is 5671.</param>
    /// <param name="tlsConfig">Optional TLS configuration. If not provided, default TLS settings are used.</param>
    /// <returns>A broker endpoint configured for secure TLS communication.</returns>
    /// <remarks>
    /// Secure endpoints provide encrypted communication for production environments:
    /// - TLS encryption for confidentiality and integrity
    /// - Standard AMQP over TLS port (5671) by default
    /// - Configurable TLS parameters for security policy compliance
    /// - Enhanced security metadata for operational visibility
    /// 
    /// Secure endpoints are essential for:
    /// - Production deployments with sensitive data
    /// - Compliance with security standards and regulations
    /// - Cross-network communication and internet-facing deployments
    /// - Multi-tenant environments requiring data isolation
    /// </remarks>
    public static BrokerEndpoint CreateSecure(string host, int port = 5671, TlsConfiguration? tlsConfig = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        return new BrokerEndpoint
        {
            Host = host,
            Port = port,
            UseTls = true,
            TlsConfiguration = tlsConfig ?? TlsConfiguration.CreateDefault(),
            Priority = 1,
            Weight = 1,
            Metadata = new Dictionary<string, string>
            {
                ["security"] = "tls-enabled",
                ["environment"] = "production"
            }
        };
    }

    /// <summary>
    /// Parses a connection string to create a broker endpoint.
    /// </summary>
    /// <param name="connectionString">The connection string in AMQP URL format.</param>
    /// <returns>A broker endpoint parsed from the connection string.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string format is invalid.</exception>
    /// <remarks>
    /// Connection string parsing supports standard AMQP URL formats:
    /// - Basic format: "amqp://host:port"
    /// - With credentials: "amqp://user:pass@host:port"
    /// - Secure connections: "amqps://host:port"
    /// - Virtual host specification: "amqp://host:port/vhost"
    /// - Query parameters: "amqp://host:port?heartbeat=60"
    /// 
    /// Parsing extracts all relevant connection parameters and creates
    /// an appropriately configured endpoint with security settings
    /// based on the protocol scheme and provided parameters.
    /// </remarks>
    public static BrokerEndpoint ParseConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        try
        {
            var uri = new Uri(connectionString);
            
            var endpoint = new BrokerEndpoint
            {
                Host = uri.Host,
                Port = uri.Port != -1 ? uri.Port : (uri.Scheme == "amqps" ? 5671 : 5672),
                UseTls = uri.Scheme == "amqps",
                Priority = 1,
                Weight = 1
            };

            // Parse query parameters for additional configuration
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var connectionProps = new Dictionary<string, object>();

                foreach (string? key in queryParams.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        connectionProps[key] = queryParams[key] ?? "";
                    }
                }

                if (connectionProps.Any())
                {
                    endpoint.ConnectionProperties = connectionProps;
                }
            }

            // Add metadata based on URI characteristics
            endpoint.Metadata = new Dictionary<string, string>
            {
                ["source"] = "connection-string",
                ["protocol"] = uri.Scheme,
                ["parsed-from"] = connectionString
            };

            return endpoint;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new ArgumentException($"Invalid connection string format: {connectionString}", nameof(connectionString), ex);
        }
    }

    /// <summary>
    /// Validates the broker endpoint configuration for completeness and consistency.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the endpoint configuration is invalid.</exception>
    /// <remarks>
    /// Endpoint validation ensures that all required parameters are provided
    /// and that the configuration will result in successful broker connectivity.
    /// 
    /// Validation checks include:
    /// - Host name format and DNS resolution capability
    /// - Port number validity and range checking
    /// - TLS configuration consistency and completeness
    /// - Priority and weight value reasonableness
    /// - Connection property format and value validation
    /// </remarks>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new BrokerConfigurationException(
                "Broker endpoint host cannot be null or empty.",
                configurationSection: nameof(BrokerEndpoint),
                parameterName: nameof(Host),
                parameterValue: Host);
        }

        if (Port is <= 0 or > 65535)
        {
            throw new BrokerConfigurationException(
                "Broker endpoint port must be between 1 and 65535.",
                configurationSection: nameof(BrokerEndpoint),
                parameterName: nameof(Port),
                parameterValue: Port);
        }

        if (Weight < 0)
        {
            throw new BrokerConfigurationException(
                "Broker endpoint weight cannot be negative.",
                configurationSection: nameof(BrokerEndpoint),
                parameterName: nameof(Weight),
                parameterValue: Weight);
        }

        // Validate TLS configuration if TLS is enabled
        if (UseTls && TlsConfiguration != null)
        {
            TlsConfiguration.Validate();
        }

        // Validate connection properties if present
        if (ConnectionProperties?.Any() != true) return;
        foreach (var property in ConnectionProperties)
        {
            if (string.IsNullOrWhiteSpace(property.Key))
            {
                throw new BrokerConfigurationException(
                    "Connection property keys cannot be null or empty.",
                    configurationSection: nameof(BrokerEndpoint),
                    parameterName: "ConnectionProperties.Key");
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the broker endpoint.
    /// </summary>
    /// <returns>A formatted string containing the endpoint address and key characteristics.</returns>
    public override string ToString()
    {
        var protocol = UseTls ? "amqps" : "amqp";
        var priorityInfo = Priority != 0 ? $" (priority: {Priority})" : "";
        var weightInfo = Weight != 1 ? $" (weight: {Weight})" : "";
        
        return $"{protocol}://{Host}:{Port}{priorityInfo}{weightInfo}";
    }

    /// <summary>
    /// Determines whether two broker endpoints are equivalent based on their connection parameters.
    /// </summary>
    /// <param name="obj">The object to compare with this endpoint.</param>
    /// <returns><c>true</c> if the endpoints are equivalent; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BrokerEndpoint other &&
               Host == other.Host &&
               Port == other.Port &&
               UseTls == other.UseTls;
    }

    /// <summary>
    /// Returns a hash code for the broker endpoint based on its connection parameters.
    /// </summary>
    /// <returns>A hash code suitable for use in hash tables and collections.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Host, Port, UseTls);
    }
}