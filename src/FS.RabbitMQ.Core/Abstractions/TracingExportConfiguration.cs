namespace FS.RabbitMQ.Core.Abstractions;

/// <summary>
/// Contains configuration for exporting traces to external tracing systems.
/// </summary>
/// <remarks>
/// Tracing export configuration enables integration with distributed tracing
/// platforms, providing comprehensive visibility into message processing
/// flows across distributed systems.
/// </remarks>
public sealed record TracingExportConfiguration
{
    /// <summary>
    /// Gets the type of tracing export system to use.
    /// </summary>
    /// <value>The export system type identifier.</value>
    public required string ExportType { get; init; }

    /// <summary>
    /// Gets the endpoint URL for trace export.
    /// </summary>
    /// <value>The URL where traces should be sent, or null if not applicable.</value>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets the batch size for trace export operations.
    /// </summary>
    /// <value>The number of spans to include in each export batch.</value>
    public int BatchSize { get; init; } = 100;

    /// <summary>
    /// Gets the export timeout for trace operations.
    /// </summary>
    /// <value>The maximum time to wait for trace export completion.</value>
    public TimeSpan ExportTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets additional configuration for the export system.
    /// </summary>
    /// <value>A dictionary containing export-specific configuration options.</value>
    public IReadOnlyDictionary<string, object>? Configuration { get; init; }

    /// <summary>
    /// Creates a tracing export configuration for Jaeger.
    /// </summary>
    /// <param name="endpoint">The Jaeger collector endpoint.</param>
    /// <param name="batchSize">The batch size for trace export.</param>
    /// <returns>A tracing export configuration for Jaeger.</returns>
    public static TracingExportConfiguration CreateJaeger(string endpoint, int batchSize = 100) =>
        new()
        {
            ExportType = "Jaeger",
            Endpoint = endpoint,
            BatchSize = batchSize
        };

    /// <summary>
    /// Creates a tracing export configuration for OpenTelemetry.
    /// </summary>
    /// <param name="endpoint">The OTLP endpoint.</param>
    /// <param name="batchSize">The batch size for trace export.</param>
    /// <returns>A tracing export configuration for OpenTelemetry.</returns>
    public static TracingExportConfiguration CreateOpenTelemetry(string endpoint, int batchSize = 100) =>
        new()
        {
            ExportType = "OpenTelemetry",
            Endpoint = endpoint,
            BatchSize = batchSize
        };
}