using FS.RabbitMQ.Core.Abstractions;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains advanced configuration options for specialized scenarios and performance tuning.
/// </summary>
/// <remarks>
/// Advanced configuration provides access to sophisticated features and performance
/// optimizations that are typically used in specialized deployments or high-performance
/// scenarios. These settings should be modified with care and thorough understanding.
/// </remarks>
public sealed class AdvancedConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether high-performance mode is enabled.
    /// </summary>
    /// <value><c>true</c> to enable high-performance optimizations; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// High-performance mode enables aggressive optimizations that may trade
    /// some reliability or observability features for improved throughput and latency.
    /// </remarks>
    public bool HighPerformanceMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the custom serialization configuration.
    /// </summary>
    /// <value>The serialization settings for message content handling.</value>
    /// <remarks>
    /// Custom serialization configuration enables optimization of message
    /// serialization for specific data types and performance requirements.
    /// </remarks>
    public SerializationConfiguration? Serialization { get; set; }

    /// <summary>
    /// Gets or sets the thread pool configuration for async operations.
    /// </summary>
    /// <value>The thread pool settings for managing concurrent operations.</value>
    /// <remarks>
    /// Thread pool configuration enables fine-tuning of concurrency and
    /// resource utilization for high-throughput scenarios.
    /// </remarks>
    public ThreadPoolConfiguration? ThreadPool { get; set; }

    /// <summary>
    /// Gets or sets the memory management configuration.
    /// </summary>
    /// <value>The memory management settings for optimizing resource usage.</value>
    /// <remarks>
    /// Memory management configuration enables optimization of memory usage
    /// patterns and garbage collection behavior for specific workloads.
    /// </remarks>
    public MemoryConfiguration? Memory { get; set; }

    /// <summary>
    /// Gets or sets experimental features configuration.
    /// </summary>
    /// <value>Settings for experimental and preview features.</value>
    /// <remarks>
    /// Experimental features provide early access to new capabilities
    /// that may not be fully stabilized or supported in production environments.
    /// </remarks>
    public ExperimentalFeaturesConfiguration? ExperimentalFeatures { get; set; }

    /// <summary>
    /// Gets or sets custom advanced properties.
    /// </summary>
    /// <value>A dictionary of advanced configuration properties.</value>
    /// <remarks>
    /// Custom properties enable configuration of internal behavior and
    /// integration with specialized infrastructure or monitoring systems.
    /// </remarks>
    public IDictionary<string, object>? CustomProperties { get; set; }

    /// <summary>
    /// Creates an advanced configuration suitable for development environments.
    /// </summary>
    /// <returns>An advanced configuration with development-friendly defaults.</returns>
    public static AdvancedConfiguration CreateDevelopment()
    {
        return new AdvancedConfiguration
        {
            HighPerformanceMode = false,
            Serialization = SerializationConfiguration.CreateDefault(),
            ExperimentalFeatures = ExperimentalFeaturesConfiguration.CreateDevelopment()
        };
    }

    /// <summary>
    /// Creates an advanced configuration suitable for production environments.
    /// </summary>
    /// <returns>An advanced configuration with production-ready defaults.</returns>
    public static AdvancedConfiguration CreateProduction()
    {
        return new AdvancedConfiguration
        {
            HighPerformanceMode = false,
            Serialization = SerializationConfiguration.CreateProduction(),
            ThreadPool = ThreadPoolConfiguration.CreateDefault(),
            Memory = MemoryConfiguration.CreateDefault()
        };
    }

    /// <summary>
    /// Creates an advanced configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <returns>An advanced configuration with performance optimizations.</returns>
    public static AdvancedConfiguration CreateHighThroughput()
    {
        return new AdvancedConfiguration
        {
            HighPerformanceMode = true,
            Serialization = SerializationConfiguration.CreateHighPerformance(),
            ThreadPool = ThreadPoolConfiguration.CreateHighThroughput(),
            Memory = MemoryConfiguration.CreateHighThroughput()
        };
    }

    /// <summary>
    /// Validates the advanced configuration for consistency and safety.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid or potentially unsafe.</exception>
    public void Validate()
    {
        Serialization?.Validate();
        ThreadPool?.Validate();
        Memory?.Validate();
        ExperimentalFeatures?.Validate();

        if (CustomProperties?.Any() == true)
        {
            foreach (var property in CustomProperties)
            {
                if (string.IsNullOrWhiteSpace(property.Key))
                {
                    throw new BrokerConfigurationException(
                        "Custom property keys cannot be null or empty.",
                        configurationSection: nameof(AdvancedConfiguration),
                        parameterName: "CustomProperties.Key");
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the advanced configuration.
    /// </summary>
    /// <returns>A formatted string describing the advanced settings.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        if (HighPerformanceMode) features.Add("high-perf");
        if (Serialization != null) features.Add("custom-serialization");
        if (ThreadPool != null) features.Add("custom-threads");
        if (Memory != null) features.Add("memory-tuning");
        if (ExperimentalFeatures != null) features.Add("experimental");

        return $"Advanced[{string.Join(", ", features)}]";
    }
}