namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration options for monitoring and observability features.
/// </summary>
/// <remarks>
/// Monitoring options control the collection and reporting of metrics, traces,
/// and health information, enabling comprehensive observability for messaging
/// operations and integration with monitoring and APM systems.
/// </remarks>
public sealed class MonitoringOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable performance metrics collection.
    /// </summary>
    /// <value><c>true</c> to collect performance metrics; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable distributed tracing.
    /// </summary>
    /// <value><c>true</c> to enable distributed tracing; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable health checks.
    /// </summary>
    /// <value><c>true</c> to enable health checks; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval for collecting and reporting metrics.
    /// </summary>
    /// <value>The duration between metric collection cycles. Default is 30 seconds.</value>
    public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the sampling rate for distributed tracing.
    /// </summary>
    /// <value>The percentage of operations to trace (0.0 to 1.0). Default is 0.1 (10%).</value>
    public double TracingSamplingRate { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets custom tags to include with all metrics and traces.
    /// </summary>
    /// <value>A dictionary of custom tags for observability data.</value>
    public IDictionary<string, string>? CustomTags { get; set; }

    /// <summary>
    /// Gets or sets the metrics export configuration.
    /// </summary>
    /// <value>Configuration for exporting metrics to external systems, or null to use default export.</value>
    public MetricsExportConfiguration? MetricsExport { get; set; }

    /// <summary>
    /// Gets or sets the tracing export configuration.
    /// </summary>
    /// <value>Configuration for exporting traces to external systems, or null to use default export.</value>
    public TracingExportConfiguration? TracingExport { get; set; }

    /// <summary>
    /// Creates monitoring options optimized for production environments.
    /// </summary>
    /// <param name="enableAll">Whether to enable all monitoring features.</param>
    /// <returns>Monitoring options configured for production use.</returns>
    public static MonitoringOptions CreateProduction(bool enableAll = true) =>
        new()
        {
            EnableMetrics = enableAll,
            EnableTracing = enableAll,
            EnableHealthChecks = enableAll,
            MetricsInterval = TimeSpan.FromSeconds(30),
            TracingSamplingRate = 0.05 // 5% sampling for production
        };

    /// <summary>
    /// Creates monitoring options optimized for development environments.
    /// </summary>
    /// <param name="enableVerboseTracing">Whether to enable verbose tracing for debugging.</param>
    /// <returns>Monitoring options configured for development use.</returns>
    public static MonitoringOptions CreateDevelopment(bool enableVerboseTracing = true) =>
        new()
        {
            EnableMetrics = true,
            EnableTracing = true,
            EnableHealthChecks = true,
            MetricsInterval = TimeSpan.FromSeconds(10),
            TracingSamplingRate = enableVerboseTracing ? 1.0 : 0.1 // 100% or 10% sampling
        };
}