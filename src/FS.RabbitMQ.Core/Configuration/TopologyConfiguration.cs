using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for messaging topology management and declaration.
/// </summary>
/// <remarks>
/// Topology configuration enables declarative definition of exchanges, queues, bindings,
/// and their relationships. It supports infrastructure-as-code approaches and automated
/// environment setup while providing flexibility for different deployment strategies.
/// </remarks>
public sealed class TopologyConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether topology should be automatically declared.
    /// </summary>
    /// <value><c>true</c> to automatically declare topology; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Automatic topology declaration creates exchanges, queues, and bindings
    /// automatically when they are first used, simplifying application deployment
    /// and reducing manual infrastructure setup requirements.
    /// </remarks>
    public bool AutoDeclareTopology { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether topology should be verified on startup.
    /// </summary>
    /// <value><c>true</c> to verify topology on startup; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Topology verification ensures that required infrastructure exists and
    /// is correctly configured before beginning message operations, providing
    /// early detection of configuration issues.
    /// </remarks>
    public bool VerifyTopologyOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the default exchange configuration.
    /// </summary>
    /// <value>The default settings for exchange creation.</value>
    /// <remarks>
    /// Default exchange configuration provides consistent settings for
    /// automatically created exchanges, ensuring proper durability,
    /// routing behavior, and operational characteristics.
    /// </remarks>
    public ExchangeConfiguration DefaultExchange { get; set; } = new();

    /// <summary>
    /// Gets or sets the default queue configuration.
    /// </summary>
    /// <value>The default settings for queue creation.</value>
    /// <remarks>
    /// Default queue configuration provides consistent settings for
    /// automatically created queues, ensuring proper durability,
    /// message handling, and performance characteristics.
    /// </remarks>
    public QueueConfiguration DefaultQueue { get; set; } = new();

    /// <summary>
    /// Gets or sets the retry policy for topology operations.
    /// </summary>
    /// <value>The retry policy for handling topology declaration failures.</value>
    /// <remarks>
    /// Topology retry policy defines how the system retries failed topology
    /// operations, providing resilience against transient broker issues
    /// during infrastructure setup.
    /// </remarks>
    public RetryPolicy? TopologyRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the topology operation timeout.
    /// </summary>
    /// <value>The maximum time allowed for topology operations. Default is 30 seconds.</value>
    /// <remarks>
    /// Topology timeout prevents hung topology operations from blocking
    /// application startup and provides predictable behavior during
    /// infrastructure initialization.
    /// </remarks>
    public TimeSpan TopologyTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether topology should be validated for consistency.
    /// </summary>
    /// <value><c>true</c> to validate topology consistency; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Topology validation ensures that exchange and queue configurations
    /// are consistent with application requirements and broker capabilities,
    /// preventing runtime errors due to configuration mismatches.
    /// </remarks>
    public bool ValidateTopologyConsistency { get; set; } = true;

    /// <summary>
    /// Gets or sets the predefined topology declarations.
    /// </summary>
    /// <value>A collection of topology declarations to be created on startup.</value>
    /// <remarks>
    /// Predefined topology enables infrastructure-as-code approaches where
    /// the complete messaging topology is declared in configuration and
    /// automatically created during application initialization.
    /// </remarks>
    public IList<TopologyDeclaration>? PredefinedTopology { get; set; }

    /// <summary>
    /// Creates a topology configuration suitable for development environments.
    /// </summary>
    /// <returns>A topology configuration with development-friendly defaults.</returns>
    public static TopologyConfiguration CreateDevelopment()
    {
        return new TopologyConfiguration
        {
            AutoDeclareTopology = true,
            VerifyTopologyOnStartup = false, // Skip verification for faster startup
            DefaultExchange = ExchangeConfiguration.CreateDevelopment(),
            DefaultQueue = QueueConfiguration.CreateDevelopment(),
            TopologyRetryPolicy = RetryPolicy.CreateImmediate(maxAttempts: 3),
            TopologyTimeout = TimeSpan.FromSeconds(10),
            ValidateTopologyConsistency = false // Relaxed for development
        };
    }

    /// <summary>
    /// Creates a topology configuration suitable for production environments.
    /// </summary>
    /// <returns>A topology configuration with production-ready defaults.</returns>
    public static TopologyConfiguration CreateProduction()
    {
        return new TopologyConfiguration
        {
            AutoDeclareTopology = true,
            VerifyTopologyOnStartup = true,
            DefaultExchange = ExchangeConfiguration.CreateProduction(),
            DefaultQueue = QueueConfiguration.CreateProduction(),
            TopologyRetryPolicy = RetryPolicy.CreateExponentialBackoff(
                baseDelay: TimeSpan.FromSeconds(2),
                maxDelay: TimeSpan.FromMinutes(2),
                maxAttempts: 5),
            TopologyTimeout = TimeSpan.FromSeconds(30),
            ValidateTopologyConsistency = true
        };
    }

    /// <summary>
    /// Creates a default topology configuration.
    /// </summary>
    /// <returns>A topology configuration with balanced defaults.</returns>
    public static TopologyConfiguration CreateDefault()
    {
        return new TopologyConfiguration
        {
            AutoDeclareTopology = true,
            VerifyTopologyOnStartup = true,
            DefaultExchange = ExchangeConfiguration.CreateDefault(),
            DefaultQueue = QueueConfiguration.CreateDefault(),
            TopologyRetryPolicy = RetryPolicy.CreateExponentialBackoff(maxAttempts: 3),
            TopologyTimeout = TimeSpan.FromSeconds(30),
            ValidateTopologyConsistency = true
        };
    }

    /// <summary>
    /// Validates the topology configuration for consistency and completeness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (TopologyTimeout <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Topology timeout must be positive.",
                configurationSection: nameof(TopologyConfiguration),
                parameterName: nameof(TopologyTimeout),
                parameterValue: TopologyTimeout);
        }

        DefaultExchange.Validate();
        DefaultQueue.Validate();

        if (PredefinedTopology?.Any() == true)
        {
            foreach (var declaration in PredefinedTopology)
            {
                declaration.Validate();
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the topology configuration.
    /// </summary>
    /// <returns>A formatted string describing the topology settings.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        if (AutoDeclareTopology) features.Add("auto-declare");
        if (VerifyTopologyOnStartup) features.Add("startup-verify");
        if (ValidateTopologyConsistency) features.Add("consistency-check");
        if (PredefinedTopology?.Any() == true) features.Add($"{PredefinedTopology.Count} predefined");

        return $"Topology[{string.Join(", ", features)}]";
    }
}