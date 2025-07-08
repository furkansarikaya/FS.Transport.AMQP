using FS.RabbitMQ.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Core.Configuration;

/// <summary>
/// Contains configuration for monitoring, metrics collection, and observability.
/// </summary>
/// <remarks>
/// Monitoring configuration enables comprehensive observability for messaging operations,
/// supporting integration with various monitoring platforms and providing detailed
/// insights into system behavior and performance.
/// </remarks>
public sealed class MonitoringConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether metrics collection is enabled.
    /// </summary>
    /// <value><c>true</c> to enable metrics collection; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Metrics collection provides quantitative insights into system performance,
    /// including message rates, processing times, error rates, and resource utilization.
    /// </remarks>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval for metrics collection and reporting.
    /// </summary>
    /// <value>The interval between metrics collection cycles. Default is 30 seconds.</value>
    /// <remarks>
    /// Metrics interval balances observability granularity with system overhead.
    /// Shorter intervals provide more detailed insights but increase overhead.
    /// </remarks>
    public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether distributed tracing is enabled.
    /// </summary>
    /// <value><c>true</c> to enable distributed tracing; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    /// <remarks>
    /// Distributed tracing enables end-to-end request tracking across multiple
    /// services and components, providing insights into message flow and latency.
    /// </remarks>
    public bool EnableTracing { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether health checks are enabled.
    /// </summary>
    /// <value><c>true</c> to enable health checks; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Health checks provide operational visibility into system health and
    /// enable automated health monitoring and alerting systems.
    /// </remarks>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval.
    /// </summary>
    /// <value>The interval between health check executions. Default is 30 seconds.</value>
    /// <remarks>
    /// Health check interval determines how frequently the system validates
    /// its operational health and connectivity to dependencies.
    /// </remarks>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether performance counters are enabled.
    /// </summary>
    /// <value><c>true</c> to enable performance counters; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    /// <remarks>
    /// Performance counters provide real-time insights into system performance
    /// characteristics and resource utilization patterns.
    /// </remarks>
    public bool EnablePerformanceCounters { get; set; } = true;

    /// <summary>
    /// Gets or sets the log level for monitoring and diagnostic information.
    /// </summary>
    /// <value>The minimum log level for monitoring messages. Default is Information.</value>
    /// <remarks>
    /// Log level controls the verbosity of monitoring and diagnostic output,
    /// balancing observability needs with log volume and storage costs.
    /// </remarks>
    public LogLevel MonitoringLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets custom monitoring tags for resource identification.
    /// </summary>
    /// <value>A dictionary of custom tags applied to monitoring data.</value>
    /// <remarks>
    /// Custom tags enable segmentation and filtering of monitoring data
    /// based on environment, application, or operational characteristics.
    /// </remarks>
    public IDictionary<string, string>? CustomTags { get; set; }

    /// <summary>
    /// Creates a monitoring configuration suitable for development environments.
    /// </summary>
    /// <returns>A monitoring configuration with development-friendly defaults.</returns>
    public static MonitoringConfiguration CreateDevelopment()
    {
        return new MonitoringConfiguration
        {
            EnableMetrics = true,
            MetricsInterval = TimeSpan.FromSeconds(10),
            EnableTracing = true, // Useful for debugging
            EnableHealthChecks = true,
            HealthCheckInterval = TimeSpan.FromSeconds(15),
            EnablePerformanceCounters = false, // Reduce overhead
            MonitoringLogLevel = LogLevel.Debug,
            CustomTags = new Dictionary<string, string>
            {
                ["environment"] = "development"
            }
        };
    }

    /// <summary>
    /// Creates a monitoring configuration suitable for production environments.
    /// </summary>
    /// <returns>A monitoring configuration with production-ready defaults.</returns>
    public static MonitoringConfiguration CreateProduction()
    {
        return new MonitoringConfiguration
        {
            EnableMetrics = true,
            MetricsInterval = TimeSpan.FromMinutes(1),
            EnableTracing = true,
            EnableHealthChecks = true,
            HealthCheckInterval = TimeSpan.FromSeconds(30),
            EnablePerformanceCounters = true,
            MonitoringLogLevel = LogLevel.Information,
            CustomTags = new Dictionary<string, string>
            {
                ["environment"] = "production"
            }
        };
    }

    /// <summary>
    /// Creates a monitoring configuration optimized for high-throughput scenarios.
    /// </summary>
    /// <returns>A monitoring configuration with minimal overhead.</returns>
    public static MonitoringConfiguration CreateHighThroughput()
    {
        return new MonitoringConfiguration
        {
            EnableMetrics = true,
            MetricsInterval = TimeSpan.FromMinutes(5), // Reduced frequency
            EnableTracing = false, // Disable for performance
            EnableHealthChecks = true,
            HealthCheckInterval = TimeSpan.FromMinutes(1),
            EnablePerformanceCounters = false, // Reduced overhead
            MonitoringLogLevel = LogLevel.Warning, // Minimal logging
            CustomTags = new Dictionary<string, string>
            {
                ["environment"] = "high-throughput"
            }
        };
    }

    /// <summary>
    /// Validates the monitoring configuration for consistency and reasonableness.
    /// </summary>
    /// <exception cref="BrokerConfigurationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (MetricsInterval <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Metrics interval must be positive.",
                configurationSection: nameof(MonitoringConfiguration),
                parameterName: nameof(MetricsInterval),
                parameterValue: MetricsInterval);
        }

        if (HealthCheckInterval <= TimeSpan.Zero)
        {
            throw new BrokerConfigurationException(
                "Health check interval must be positive.",
                configurationSection: nameof(MonitoringConfiguration),
                parameterName: nameof(HealthCheckInterval),
                parameterValue: HealthCheckInterval);
        }

        if (CustomTags?.Any() == true)
        {
            foreach (var tag in CustomTags)
            {
                if (string.IsNullOrWhiteSpace(tag.Key))
                {
                    throw new BrokerConfigurationException(
                        "Custom tag keys cannot be null or empty.",
                        configurationSection: nameof(MonitoringConfiguration),
                        parameterName: "CustomTags.Key");
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the monitoring configuration.
    /// </summary>
    /// <returns>A formatted string describing the monitoring settings.</returns>
    public override string ToString()
    {
        var features = new List<string>();
        if (EnableMetrics) features.Add("metrics");
        if (EnableTracing) features.Add("tracing");
        if (EnableHealthChecks) features.Add("health-checks");
        if (EnablePerformanceCounters) features.Add("perf-counters");

        return $"Monitoring[{string.Join(", ", features)}, interval: {MetricsInterval}]";
    }
}
