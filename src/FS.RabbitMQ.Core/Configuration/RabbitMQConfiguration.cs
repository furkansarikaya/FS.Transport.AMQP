using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains comprehensive configuration for RabbitMQ broker connections and messaging behavior.
/// </summary>
/// <remarks>
/// The RabbitMQ configuration serves as the central configuration point for all messaging
/// operations within the application. It encompasses connection parameters, performance tuning,
/// security settings, and operational policies that govern how the messaging infrastructure
/// behaves under various conditions.
/// 
/// This configuration follows the principle of "configuration over convention" while providing
/// sensible defaults for common scenarios. It supports both simple development setups and
/// sophisticated production deployments with multiple brokers, advanced security, and
/// fine-tuned performance characteristics.
/// 
/// Key design principles:
/// - Hierarchical configuration with environment-specific overrides
/// - Validation of configuration consistency and completeness
/// - Support for dynamic reconfiguration and hot-reloading
/// - Integration with .NET configuration system and dependency injection
/// - Comprehensive documentation for operational teams
/// </remarks>
public sealed class RabbitMQConfiguration
{
    /// <summary>
    /// Gets or sets the connection configuration for broker connectivity.
    /// </summary>
    /// <value>The connection settings that define how to connect to RabbitMQ brokers.</value>
    /// <remarks>
    /// Connection configuration is the foundation of all messaging operations.
    /// It defines the network-level parameters, authentication credentials,
    /// and connection behavior that enables communication with RabbitMQ brokers.
    /// 
    /// This configuration supports both single-broker and clustered deployments,
    /// with automatic failover, load balancing, and connection recovery capabilities.
    /// </remarks>
    public ConnectionConfiguration Connection { get; set; } = new();

    /// <summary>
    /// Gets or sets the publisher configuration for message sending operations.
    /// </summary>
    /// <value>The settings that control message publishing behavior and guarantees.</value>
    /// <remarks>
    /// Publisher configuration defines the default behavior for message publishing
    /// operations, including delivery guarantees, serialization settings, and
    /// performance optimizations. These settings can be overridden at the individual
    /// message level for fine-grained control.
    /// </remarks>
    public PublisherConfiguration Publisher { get; set; } = new();

    /// <summary>
    /// Gets or sets the consumer configuration for message receiving operations.
    /// </summary>
    /// <value>The settings that control message consumption behavior and quality of service.</value>
    /// <remarks>
    /// Consumer configuration establishes the default behavior for all consumer types,
    /// including performance characteristics, error handling policies, and resource
    /// management settings. Individual consumers can override these defaults based
    /// on their specific requirements.
    /// </remarks>
    public ConsumerConfiguration Consumer { get; set; } = new();

    /// <summary>
    /// Gets or sets the topology configuration for exchanges, queues, and bindings.
    /// </summary>
    /// <value>The settings that define the messaging topology and infrastructure setup.</value>
    /// <remarks>
    /// Topology configuration enables declarative definition of the messaging
    /// infrastructure, including exchanges, queues, bindings, and their relationships.
    /// This supports infrastructure-as-code approaches and automated environment setup.
    /// </remarks>
    public TopologyConfiguration Topology { get; set; } = new();

    /// <summary>
    /// Gets or sets the monitoring and observability configuration.
    /// </summary>
    /// <value>The settings that control metrics collection, tracing, and health monitoring.</value>
    /// <remarks>
    /// Monitoring configuration enables comprehensive observability for messaging
    /// operations, supporting integration with various monitoring platforms and
    /// providing detailed insights into system behavior and performance.
    /// </remarks>
    public MonitoringConfiguration Monitoring { get; set; } = new();

    /// <summary>
    /// Gets or sets the security configuration for authentication and authorization.
    /// </summary>
    /// <value>The settings that define security policies and access controls.</value>
    /// <remarks>
    /// Security configuration encompasses authentication mechanisms, authorization
    /// policies, encryption settings, and compliance requirements. It supports
    /// various security models from simple username/password authentication to
    /// sophisticated certificate-based mutual TLS and external identity providers.
    /// </remarks>
    public SecurityConfiguration Security { get; set; } = new();

    /// <summary>
    /// Gets or sets advanced configuration options for specialized scenarios.
    /// </summary>
    /// <value>The settings for advanced features and performance tuning.</value>
    /// <remarks>
    /// Advanced configuration provides access to sophisticated features and
    /// performance tuning options that are typically used in specialized deployments
    /// or high-performance scenarios. These settings should be modified with care
    /// and thorough understanding of their implications.
    /// </remarks>
    public AdvancedConfiguration Advanced { get; set; } = new();

    /// <summary>
    /// Validates the entire configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration contains invalid or inconsistent settings.</exception>
    /// <remarks>
    /// Configuration validation ensures that all settings are internally consistent,
    /// required parameters are provided, and the combination of settings will result
    /// in a functional messaging system. Validation is performed automatically during
    /// broker initialization but can also be called explicitly for configuration testing.
    /// 
    /// Validation checks include:
    /// - Required parameter presence and format validation
    /// - Cross-setting consistency and compatibility
    /// - Resource limit and capacity validation
    /// - Security configuration completeness
    /// - Performance setting sanity checks
    /// </remarks>
    public void Validate()
    {
        try
        {
            Connection.Validate();
            Publisher.Validate();
            Consumer.Validate();
            Topology.Validate();
            Monitoring.Validate();
            Security.Validate();
            Advanced.Validate();

            // Cross-configuration validation
            ValidateCrossConfigurationConsistency();
        }
        catch (Exception ex) when (!(ex is BrokerConfigurationException))
        {
            throw new BrokerConfigurationException(
                "Configuration validation failed due to invalid settings.",
                innerException: ex,
                configurationSection: "RabbitMQConfiguration");
        }
    }

    /// <summary>
    /// Creates a configuration optimized for development environments.
    /// </summary>
    /// <param name="connectionString">The RabbitMQ connection string for the development broker.</param>
    /// <returns>A RabbitMQ configuration with development-friendly defaults.</returns>
    /// <remarks>
    /// Development configuration prioritizes ease of use, debugging capabilities,
    /// and rapid iteration over performance and production-ready features:
    /// 
    /// - Simplified connection settings with local broker defaults
    /// - Verbose logging and monitoring for debugging
    /// - Relaxed security settings for development convenience
    /// - Conservative resource limits to prevent local system overload
    /// - Automatic topology creation for rapid prototyping
    /// 
    /// This configuration should never be used in production environments
    /// due to security and performance implications.
    /// </remarks>
    public static RabbitMQConfiguration CreateDevelopment(string? connectionString = null)
    {
        return new RabbitMQConfiguration
        {
            Connection = ConnectionConfiguration.CreateDevelopment(connectionString),
            Publisher = PublisherConfiguration.CreateDevelopment(),
            Consumer = ConsumerConfiguration.CreateDevelopment(),
            Topology = TopologyConfiguration.CreateDevelopment(),
            Monitoring = MonitoringConfiguration.CreateDevelopment(),
            Security = SecurityConfiguration.CreateDevelopment(),
            Advanced = AdvancedConfiguration.CreateDevelopment()
        };
    }

    /// <summary>
    /// Creates a configuration optimized for production environments.
    /// </summary>
    /// <param name="connectionString">The RabbitMQ connection string for the production broker cluster.</param>
    /// <returns>A RabbitMQ configuration with production-ready defaults.</returns>
    /// <remarks>
    /// Production configuration prioritizes reliability, security, performance,
    /// and operational excellence:
    /// 
    /// - Robust connection settings with clustering and failover support
    /// - Comprehensive monitoring and observability features
    /// - Strong security policies and access controls
    /// - Optimized resource utilization and performance settings
    /// - Conservative defaults that favor stability over convenience
    /// 
    /// This configuration requires careful review and customization based on
    /// specific production requirements, security policies, and performance goals.
    /// </remarks>
    public static RabbitMQConfiguration CreateProduction(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return new RabbitMQConfiguration
        {
            Connection = ConnectionConfiguration.CreateProduction(connectionString),
            Publisher = PublisherConfiguration.CreateProduction(),
            Consumer = ConsumerConfiguration.CreateProduction(),
            Topology = TopologyConfiguration.CreateProduction(),
            Monitoring = MonitoringConfiguration.CreateProduction(),
            Security = SecurityConfiguration.CreateProduction(),
            Advanced = AdvancedConfiguration.CreateProduction()
        };
    }

    /// <summary>
    /// Creates a configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <param name="connectionString">The RabbitMQ connection string for the high-performance broker cluster.</param>
    /// <returns>A RabbitMQ configuration optimized for maximum throughput.</returns>
    /// <remarks>
    /// High-throughput configuration prioritizes message processing speed and
    /// system capacity over other considerations:
    /// 
    /// - Optimized connection pooling and channel management
    /// - Batch processing and bulk operation preferences
    /// - Aggressive resource utilization and performance tuning
    /// - Minimal latency overhead in critical processing paths
    /// - Streamlined monitoring focused on performance metrics
    /// 
    /// This configuration should be used in scenarios where message volume
    /// is extremely high and latency requirements are stringent. Careful
    /// capacity planning and performance testing are essential.
    /// </remarks>
    public static RabbitMQConfiguration CreateHighThroughput(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return new RabbitMQConfiguration
        {
            Connection = ConnectionConfiguration.CreateHighThroughput(connectionString),
            Publisher = PublisherConfiguration.CreateHighThroughput(),
            Consumer = ConsumerConfiguration.CreateHighThroughput(),
            Topology = TopologyConfiguration.CreateDefault(),
            Monitoring = MonitoringConfiguration.CreateHighThroughput(),
            Security = SecurityConfiguration.CreateDefault(),
            Advanced = AdvancedConfiguration.CreateHighThroughput()
        };
    }

    /// <summary>
    /// Validates cross-configuration consistency and compatibility.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when configurations are inconsistent or incompatible.</exception>
    private void ValidateCrossConfigurationConsistency()
    {
        // Validate connection pool size vs consumer concurrency
        if (Consumer.DefaultMaxConcurrency > Connection.MaxChannelsPerConnection * Connection.ConnectionPoolSize)
        {
            throw new BrokerConfigurationException(
                "Consumer concurrency exceeds available channel capacity. " +
                $"Consumer concurrency: {Consumer.DefaultMaxConcurrency}, " +
                $"Available channels: {Connection.MaxChannelsPerConnection * Connection.ConnectionPoolSize}",
                configurationSection: "CrossConfiguration");
        }

        // Validate monitoring and advanced configuration compatibility
        if (Monitoring.EnableMetrics && Advanced.HighPerformanceMode && Monitoring.MetricsInterval < TimeSpan.FromMinutes(1))
        {
            throw new BrokerConfigurationException(
                "High-performance mode is incompatible with frequent metrics collection. " +
                "Increase metrics interval or disable high-performance mode.",
                configurationSection: "CrossConfiguration");
        }

        // Validate security and connection configuration consistency
        if (Security.RequireTls && !Connection.Endpoints.Any(e => e.UseTls))
        {
            throw new BrokerConfigurationException(
                "TLS is required by security configuration but no TLS endpoints are configured.",
                configurationSection: "CrossConfiguration");
        }
    }

    /// <summary>
    /// Returns a string representation of the configuration summary.
    /// </summary>
    /// <returns>A formatted string containing key configuration highlights.</returns>
    public override string ToString()
    {
        var endpoints = Connection.Endpoints.Count;
        var features = new List<string>();
        
        if (Security.RequireTls) features.Add("TLS");
        if (Monitoring.EnableMetrics) features.Add("metrics");
        if (Advanced.HighPerformanceMode) features.Add("high-perf");
        if (Topology.AutoDeclareTopology) features.Add("auto-topology");
        
        var featureInfo = features.Any() ? $" [{string.Join(", ", features)}]" : "";
        return $"RabbitMQ Config: {endpoints} endpoint(s), pool size {Connection.ConnectionPoolSize}{featureInfo}";
    }
}