namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for exporting metrics to external monitoring systems.
/// </summary>
/// <remarks>
/// Metrics export configuration enables integration with various monitoring
/// and observability platforms, providing flexible options for metric
/// collection, aggregation, and visualization.
/// </remarks>
public sealed record MetricsExportConfiguration
{
    /// <summary>
    /// Gets the type of metrics export system to use.
    /// </summary>
    /// <value>The export system type identifier.</value>
    public required string ExportType { get; init; }

    /// <summary>
    /// Gets the endpoint URL for metrics export.
    /// </summary>
    /// <value>The URL where metrics should be sent, or null if not applicable.</value>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets the export interval for metrics.
    /// </summary>
    /// <value>The frequency of metrics export operations.</value>
    public TimeSpan ExportInterval { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets additional configuration for the export system.
    /// </summary>
    /// <value>A dictionary containing export-specific configuration options.</value>
    public IReadOnlyDictionary<string, object>? Configuration { get; init; }

    /// <summary>
    /// Creates a metrics export configuration for Prometheus.
    /// </summary>
    /// <param name="endpoint">The Prometheus pushgateway endpoint.</param>
    /// <param name="exportInterval">The export interval.</param>
    /// <returns>A metrics export configuration for Prometheus.</returns>
    public static MetricsExportConfiguration CreatePrometheus(string? endpoint = null, TimeSpan? exportInterval = null) =>
        new()
        {
            ExportType = "Prometheus",
            Endpoint = endpoint,
            ExportInterval = exportInterval ?? TimeSpan.FromMinutes(1)
        };
}