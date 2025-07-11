using System.Collections.Concurrent;
using System.Diagnostics;
using FS.RabbitMQ.Connection;
using Microsoft.Extensions.Logging;

namespace FS.RabbitMQ.Monitoring;

/// <summary>
/// Provides comprehensive metrics collection for RabbitMQ transport components.
/// Collects connection metrics, message throughput, performance statistics, and error rates.
/// Supports custom metrics and provides real-time monitoring capabilities.
/// </summary>
public interface IMetricsCollector : IDisposable
{
    /// <summary>
    /// Gets all collected metrics as a read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<string, Metric> Metrics { get; }

    /// <summary>
    /// Gets metrics for a specific category.
    /// </summary>
    /// <param name="category">The metrics category to retrieve.</param>
    /// <returns>A dictionary containing metrics for the specified category.</returns>
    IReadOnlyDictionary<string, Metric> GetMetrics(MetricCategory category);

    /// <summary>
    /// Gets a specific metric by name.
    /// </summary>
    /// <param name="name">The name of the metric to retrieve.</param>
    /// <returns>The metric if found; otherwise, null.</returns>
    Metric? GetMetric(string name);

    /// <summary>
    /// Records a counter metric (incremental value).
    /// </summary>
    /// <param name="name">The name of the counter metric.</param>
    /// <param name="value">The value to add to the counter.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    void RecordCounter(string name, long value = 1, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a gauge metric (current value).
    /// </summary>
    /// <param name="name">The name of the gauge metric.</param>
    /// <param name="value">The current value of the gauge.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    void RecordGauge(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a histogram metric (distribution of values).
    /// </summary>
    /// <param name="name">The name of the histogram metric.</param>
    /// <param name="value">The value to record in the histogram.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a timer metric (duration measurement).
    /// </summary>
    /// <param name="name">The name of the timer metric.</param>
    /// <param name="duration">The duration to record.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    void RecordTimer(string name, TimeSpan duration, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Creates a timer scope for automatic duration measurement.
    /// </summary>
    /// <param name="name">The name of the timer metric.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <returns>A disposable timer scope.</returns>
    IDisposable StartTimer(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Resets all metrics to their initial values.
    /// </summary>
    void ResetMetrics();

    /// <summary>
    /// Resets metrics for a specific category.
    /// </summary>
    /// <param name="category">The category of metrics to reset.</param>
    void ResetMetrics(MetricCategory category);

    /// <summary>
    /// Exports metrics in the specified format.
    /// </summary>
    /// <param name="format">The format to export metrics in.</param>
    /// <returns>The exported metrics as a string.</returns>
    Task<string> ExportMetricsAsync(MetricExportFormat format = MetricExportFormat.Json);

    /// <summary>
    /// Occurs when a metric is updated.
    /// </summary>
    event EventHandler<MetricUpdatedEventArgs>? MetricUpdated;
}

/// <summary>
/// Implements comprehensive metrics collection for RabbitMQ transport components.
/// Provides built-in metrics for connection, message processing, and performance monitoring.
/// </summary>
public class MetricsCollector : IMetricsCollector
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<MetricsCollector> _logger;
    private readonly ConcurrentDictionary<string, Metric> _metrics;
    private readonly Timer _collectionTimer;
    private readonly object _lock = new object();
    private bool _disposed;

    private static readonly TimeSpan DefaultCollectionInterval = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollector"/> class.
    /// </summary>
    /// <param name="connectionManager">The connection manager to monitor.</param>
    /// <param name="logger">The logger for metrics collection operations.</param>
    public MetricsCollector(IConnectionManager connectionManager, ILogger<MetricsCollector> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = new ConcurrentDictionary<string, Metric>();

        InitializeBuiltInMetrics();
        
        // Start metrics collection timer
        _collectionTimer = new Timer(CollectSystemMetrics, null, DefaultCollectionInterval, DefaultCollectionInterval);
        
        _logger.LogInformation("MetricsCollector initialized with collection interval: {Interval}", DefaultCollectionInterval);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, Metric> Metrics => _metrics;

    /// <inheritdoc />
    public event EventHandler<MetricUpdatedEventArgs>? MetricUpdated;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, Metric> GetMetrics(MetricCategory category)
    {
        ThrowIfDisposed();
        
        return _metrics
            .Where(kvp => kvp.Value.Category == category)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <inheritdoc />
    public Metric? GetMetric(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            return null;
        
        _metrics.TryGetValue(name, out var metric);
        return metric;
    }

    /// <inheritdoc />
    public void RecordCounter(string name, long value = 1, Dictionary<string, string>? tags = null)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Metric name cannot be null or whitespace", nameof(name));
        
        var metric = _metrics.AddOrUpdate(name, 
            CreateCounterMetric(name, value, tags),
            (_, existing) => UpdateCounterMetric(existing, value));
        
        OnMetricUpdated(metric);
    }

    /// <inheritdoc />
    public void RecordGauge(string name, double value, Dictionary<string, string>? tags = null)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Metric name cannot be null or whitespace", nameof(name));
        
        var metric = _metrics.AddOrUpdate(name,
            CreateGaugeMetric(name, value, tags),
            (_, existing) => UpdateGaugeMetric(existing, value));
        
        OnMetricUpdated(metric);
    }

    /// <inheritdoc />
    public void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Metric name cannot be null or whitespace", nameof(name));
        
        var metric = _metrics.AddOrUpdate(name,
            CreateHistogramMetric(name, value, tags),
            (_, existing) => UpdateHistogramMetric(existing, value));
        
        OnMetricUpdated(metric);
    }

    /// <inheritdoc />
    public void RecordTimer(string name, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        ThrowIfDisposed();
        
        RecordHistogram(name, duration.TotalMilliseconds, tags);
    }

    /// <inheritdoc />
    public IDisposable StartTimer(string name, Dictionary<string, string>? tags = null)
    {
        ThrowIfDisposed();
        
        return new TimerScope(this, name, tags);
    }

    /// <inheritdoc />
    public void ResetMetrics()
    {
        ThrowIfDisposed();
        
        lock (_lock)
        {
            _metrics.Clear();
            InitializeBuiltInMetrics();
        }
        
        _logger.LogInformation("All metrics have been reset");
    }

    /// <inheritdoc />
    public void ResetMetrics(MetricCategory category)
    {
        ThrowIfDisposed();
        
        lock (_lock)
        {
            var metricsToRemove = _metrics
                .Where(kvp => kvp.Value.Category == category)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in metricsToRemove)
            {
                _metrics.TryRemove(key, out _);
            }
            
            // Re-initialize built-in metrics for this category
            InitializeBuiltInMetrics(category);
        }
        
        _logger.LogInformation("Metrics for category {Category} have been reset", category);
    }

    /// <inheritdoc />
    public async Task<string> ExportMetricsAsync(MetricExportFormat format = MetricExportFormat.Json)
    {
        ThrowIfDisposed();
        
        return await Task.Run(() =>
        {
            switch (format)
            {
                case MetricExportFormat.Json:
                    return ExportAsJson();
                case MetricExportFormat.Prometheus:
                    return ExportAsPrometheus();
                default:
                    throw new ArgumentException($"Unsupported export format: {format}", nameof(format));
            }
        });
    }

    /// <summary>
    /// Initializes built-in metrics for RabbitMQ transport monitoring.
    /// </summary>
    /// <param name="category">Optional category to initialize. If null, initializes all categories.</param>
    private void InitializeBuiltInMetrics(MetricCategory? category = null)
    {
        if (category == null || category == MetricCategory.Connection)
        {
            // Connection metrics
            RecordGauge("connection.is_connected", 0, new Dictionary<string, string> { ["type"] = "connection" });
            RecordCounter("connection.total_connections", 0, new Dictionary<string, string> { ["type"] = "connection" });
            RecordCounter("connection.failed_connections", 0, new Dictionary<string, string> { ["type"] = "connection" });
            RecordCounter("connection.recovery_attempts", 0, new Dictionary<string, string> { ["type"] = "connection" });
            RecordGauge("connection.active_channels", 0, new Dictionary<string, string> { ["type"] = "connection" });
        }

        if (category == null || category == MetricCategory.Message)
        {
            // Message metrics
            RecordCounter("message.published_total", 0, new Dictionary<string, string> { ["type"] = "message" });
            RecordCounter("message.consumed_total", 0, new Dictionary<string, string> { ["type"] = "message" });
            RecordCounter("message.failed_total", 0, new Dictionary<string, string> { ["type"] = "message" });
            RecordCounter("message.retried_total", 0, new Dictionary<string, string> { ["type"] = "message" });
            RecordHistogram("message.publish_duration_ms", 0, new Dictionary<string, string> { ["type"] = "message" });
            RecordHistogram("message.consume_duration_ms", 0, new Dictionary<string, string> { ["type"] = "message" });
        }

        if (category == null || category == MetricCategory.Queue)
        {
            // Queue metrics
            RecordCounter("queue.declared_total", 0, new Dictionary<string, string> { ["type"] = "queue" });
            RecordCounter("queue.deleted_total", 0, new Dictionary<string, string> { ["type"] = "queue" });
            RecordGauge("queue.message_count", 0, new Dictionary<string, string> { ["type"] = "queue" });
            RecordGauge("queue.consumer_count", 0, new Dictionary<string, string> { ["type"] = "queue" });
        }

        if (category == null || category == MetricCategory.Exchange)
        {
            // Exchange metrics
            RecordCounter("exchange.declared_total", 0, new Dictionary<string, string> { ["type"] = "exchange" });
            RecordCounter("exchange.deleted_total", 0, new Dictionary<string, string> { ["type"] = "exchange" });
        }

        if (category == null || category == MetricCategory.Performance)
        {
            // Performance metrics
            RecordGauge("performance.memory_usage_mb", 0, new Dictionary<string, string> { ["type"] = "performance" });
            RecordGauge("performance.cpu_usage_percent", 0, new Dictionary<string, string> { ["type"] = "performance" });
            RecordGauge("performance.gc_gen0_collections", 0, new Dictionary<string, string> { ["type"] = "performance" });
            RecordGauge("performance.gc_gen1_collections", 0, new Dictionary<string, string> { ["type"] = "performance" });
            RecordGauge("performance.gc_gen2_collections", 0, new Dictionary<string, string> { ["type"] = "performance" });
        }
    }

    /// <summary>
    /// Collects system metrics automatically.
    /// </summary>
    /// <param name="state">The timer state.</param>
    private async void CollectSystemMetrics(object? state)
    {
        if (_disposed)
            return;

        try
        {
            await CollectConnectionMetrics();
            await CollectPerformanceMetrics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting system metrics");
        }
    }

    /// <summary>
    /// Collects connection-related metrics.
    /// </summary>
    private Task CollectConnectionMetrics()
    {
        try
        {
            var isConnected = _connectionManager.IsConnected;
            var statistics = _connectionManager.Statistics;

            RecordGauge("connection.is_connected", isConnected ? 1 : 0);
            RecordGauge("connection.total_connections", statistics.TotalConnections);
            RecordGauge("connection.failed_connections", statistics.FailedConnections);
            RecordGauge("connection.recovery_attempts", statistics.RecoveryAttempts);
            RecordGauge("connection.active_channels", statistics.ActiveChannels);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error collecting connection metrics");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Collects performance-related metrics.
    /// </summary>
    private Task CollectPerformanceMetrics()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsageMb = process.WorkingSet64 / 1024.0 / 1024.0;
            
            RecordGauge("performance.memory_usage_mb", memoryUsageMb);
            RecordGauge("performance.gc_gen0_collections", GC.CollectionCount(0));
            RecordGauge("performance.gc_gen1_collections", GC.CollectionCount(1));
            RecordGauge("performance.gc_gen2_collections", GC.CollectionCount(2));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error collecting performance metrics");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new counter metric.
    /// </summary>
    private Metric CreateCounterMetric(string name, long value, Dictionary<string, string>? tags)
    {
        return new Metric(name, MetricType.Counter, value, tags ?? new Dictionary<string, string>(), DetermineCategory(name));
    }

    /// <summary>
    /// Updates an existing counter metric.
    /// </summary>
    private Metric UpdateCounterMetric(Metric existing, long value)
    {
        if (existing.Type != MetricType.Counter)
            throw new InvalidOperationException($"Cannot update non-counter metric '{existing.Name}' as a counter");

        var currentValue = Convert.ToInt64(existing.Value);
        return existing.WithValue(currentValue + value);
    }

    /// <summary>
    /// Creates a new gauge metric.
    /// </summary>
    private Metric CreateGaugeMetric(string name, double value, Dictionary<string, string>? tags)
    {
        return new Metric(name, MetricType.Gauge, value, tags ?? new Dictionary<string, string>(), DetermineCategory(name));
    }

    /// <summary>
    /// Updates an existing gauge metric.
    /// </summary>
    private Metric UpdateGaugeMetric(Metric existing, double value)
    {
        if (existing.Type != MetricType.Gauge)
            throw new InvalidOperationException($"Cannot update non-gauge metric '{existing.Name}' as a gauge");

        return existing.WithValue(value);
    }

    /// <summary>
    /// Creates a new histogram metric.
    /// </summary>
    private Metric CreateHistogramMetric(string name, double value, Dictionary<string, string>? tags)
    {
        var histogram = new HistogramData(value);
        return new Metric(name, MetricType.Histogram, histogram, tags ?? new Dictionary<string, string>(), DetermineCategory(name));
    }

    /// <summary>
    /// Updates an existing histogram metric.
    /// </summary>
    private Metric UpdateHistogramMetric(Metric existing, double value)
    {
        if (existing.Type != MetricType.Histogram)
            throw new InvalidOperationException($"Cannot update non-histogram metric '{existing.Name}' as a histogram");

        var histogram = existing.Value as HistogramData ?? new HistogramData(value);
        return existing.WithValue(histogram.AddValue(value));
    }

    /// <summary>
    /// Determines the metric category based on the metric name.
    /// </summary>
    private static MetricCategory DetermineCategory(string name)
    {
        if (name.StartsWith("connection.", StringComparison.OrdinalIgnoreCase))
            return MetricCategory.Connection;
        if (name.StartsWith("message.", StringComparison.OrdinalIgnoreCase))
            return MetricCategory.Message;
        if (name.StartsWith("queue.", StringComparison.OrdinalIgnoreCase))
            return MetricCategory.Queue;
        if (name.StartsWith("exchange.", StringComparison.OrdinalIgnoreCase))
            return MetricCategory.Exchange;
        if (name.StartsWith("performance.", StringComparison.OrdinalIgnoreCase))
            return MetricCategory.Performance;
        
        return MetricCategory.Custom;
    }

    /// <summary>
    /// Exports metrics as JSON.
    /// </summary>
    private string ExportAsJson()
    {
        var export = new
        {
            timestamp = DateTimeOffset.UtcNow,
            metrics = _metrics.Values.Select(m => new
            {
                name = m.Name,
                type = m.Type.ToString(),
                value = m.Value,
                tags = m.Tags,
                category = m.Category.ToString(),
                last_updated = m.LastUpdated
            })
        };

        return System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Exports metrics in Prometheus format.
    /// </summary>
    private string ExportAsPrometheus()
    {
        var lines = new List<string>();
        
        foreach (var metric in _metrics.Values)
        {
            var metricName = metric.Name.Replace(".", "_");
            var help = $"# HELP {metricName} {metric.Name} metric";
            var type = $"# TYPE {metricName} {GetPrometheusType(metric.Type)}";
            
            lines.Add(help);
            lines.Add(type);
            
            if (metric.Tags.Count > 0)
            {
                var tags = string.Join(",", metric.Tags.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\""));
                lines.Add($"{metricName}{{{tags}}} {metric.Value}");
            }
            else
            {
                lines.Add($"{metricName} {metric.Value}");
            }
            
            lines.Add("");
        }
        
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Gets the Prometheus metric type for a given metric type.
    /// </summary>
    private static string GetPrometheusType(MetricType type)
    {
        return type switch
        {
            MetricType.Counter => "counter",
            MetricType.Gauge => "gauge",
            MetricType.Histogram => "histogram",
            _ => "gauge"
        };
    }

    /// <summary>
    /// Raises the MetricUpdated event.
    /// </summary>
    private void OnMetricUpdated(Metric metric)
    {
        MetricUpdated?.Invoke(this, new MetricUpdatedEventArgs(metric));
    }

    /// <summary>
    /// Throws an exception if the metrics collector has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MetricsCollector));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _collectionTimer?.Dispose();
        _disposed = true;
        
        _logger.LogInformation("MetricsCollector disposed");
    }

    /// <summary>
    /// Implements a timer scope for automatic duration measurement.
    /// </summary>
    private class TimerScope : IDisposable
    {
        private readonly MetricsCollector _collector;
        private readonly string _name;
        private readonly Dictionary<string, string>? _tags;
        private readonly Stopwatch _stopwatch;

        public TimerScope(MetricsCollector collector, string name, Dictionary<string, string>? tags)
        {
            _collector = collector;
            _name = name;
            _tags = tags;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _collector.RecordTimer(_name, _stopwatch.Elapsed, _tags);
        }
    }
}

/// <summary>
/// Represents a metric with its associated metadata.
/// </summary>
public class Metric
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Metric"/> class.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <param name="type">The type of the metric.</param>
    /// <param name="value">The value of the metric.</param>
    /// <param name="tags">The tags associated with the metric.</param>
    /// <param name="category">The category of the metric.</param>
    public Metric(string name, MetricType type, object value, Dictionary<string, string> tags, MetricCategory category)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        Category = category;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the name of the metric.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the metric.
    /// </summary>
    public MetricType Type { get; }

    /// <summary>
    /// Gets the value of the metric.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets the tags associated with the metric.
    /// </summary>
    public Dictionary<string, string> Tags { get; }

    /// <summary>
    /// Gets the category of the metric.
    /// </summary>
    public MetricCategory Category { get; }

    /// <summary>
    /// Gets the timestamp when the metric was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; }

    /// <summary>
    /// Creates a new metric with the specified value.
    /// </summary>
    /// <param name="value">The new value for the metric.</param>
    /// <returns>A new metric instance with the updated value.</returns>
    public Metric WithValue(object value)
    {
        return new Metric(Name, Type, value, Tags, Category);
    }
}

/// <summary>
/// Represents histogram data for distribution metrics.
/// </summary>
public class HistogramData
{
    private readonly object _lock = new object();
    private readonly List<double> _values = new List<double>();
    private double _sum;
    private double _min = double.MaxValue;
    private double _max = double.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistogramData"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value to add to the histogram.</param>
    public HistogramData(double initialValue)
    {
        AddValue(initialValue);
    }

    /// <summary>
    /// Gets the count of values in the histogram.
    /// </summary>
    public int Count => _values.Count;

    /// <summary>
    /// Gets the sum of all values in the histogram.
    /// </summary>
    public double Sum => _sum;

    /// <summary>
    /// Gets the minimum value in the histogram.
    /// </summary>
    public double Min => _min;

    /// <summary>
    /// Gets the maximum value in the histogram.
    /// </summary>
    public double Max => _max;

    /// <summary>
    /// Gets the average value in the histogram.
    /// </summary>
    public double Average => Count > 0 ? _sum / Count : 0;

    /// <summary>
    /// Adds a value to the histogram.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The updated histogram data.</returns>
    public HistogramData AddValue(double value)
    {
        lock (_lock)
        {
            _values.Add(value);
            _sum += value;
            _min = Math.Min(_min, value);
            _max = Math.Max(_max, value);
        }
        
        return this;
    }

    /// <summary>
    /// Gets the percentile value from the histogram.
    /// </summary>
    /// <param name="percentile">The percentile to retrieve (0-100).</param>
    /// <returns>The percentile value.</returns>
    public double GetPercentile(double percentile)
    {
        if (percentile < 0 || percentile > 100)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

        lock (_lock)
        {
            if (_values.Count == 0)
                return 0;

            var sortedValues = _values.OrderBy(v => v).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, index)];
        }
    }
}

/// <summary>
/// Represents the type of a metric.
/// </summary>
public enum MetricType
{
    /// <summary>
    /// A counter metric that only increases.
    /// </summary>
    Counter,

    /// <summary>
    /// A gauge metric that can increase or decrease.
    /// </summary>
    Gauge,

    /// <summary>
    /// A histogram metric that tracks the distribution of values.
    /// </summary>
    Histogram
}

/// <summary>
/// Represents the category of a metric.
/// </summary>
public enum MetricCategory
{
    /// <summary>
    /// Connection-related metrics.
    /// </summary>
    Connection,

    /// <summary>
    /// Message-related metrics.
    /// </summary>
    Message,

    /// <summary>
    /// Queue-related metrics.
    /// </summary>
    Queue,

    /// <summary>
    /// Exchange-related metrics.
    /// </summary>
    Exchange,

    /// <summary>
    /// Performance-related metrics.
    /// </summary>
    Performance,

    /// <summary>
    /// Custom metrics.
    /// </summary>
    Custom
}

/// <summary>
/// Represents the export format for metrics.
/// </summary>
public enum MetricExportFormat
{
    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// Prometheus format.
    /// </summary>
    Prometheus
}

/// <summary>
/// Provides data for the metric updated event.
/// </summary>
public class MetricUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetricUpdatedEventArgs"/> class.
    /// </summary>
    /// <param name="metric">The metric that was updated.</param>
    public MetricUpdatedEventArgs(Metric metric)
    {
        Metric = metric ?? throw new ArgumentNullException(nameof(metric));
    }

    /// <summary>
    /// Gets the metric that was updated.
    /// </summary>
    public Metric Metric { get; }
}