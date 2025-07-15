using FS.StreamFlow.Core.Features.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FS.StreamFlow.RabbitMQ.Features.Metrics;

/// <summary>
/// RabbitMQ implementation of metrics collector providing comprehensive monitoring and observability
/// </summary>
public class RabbitMQMetricsCollector : IMetricsCollector
{
    private readonly ILogger<RabbitMQMetricsCollector> _logger;
    private readonly ConcurrentDictionary<string, CounterMetric> _counters;
    private readonly ConcurrentDictionary<string, GaugeMetric> _gauges;
    private readonly ConcurrentDictionary<string, List<double>> _histogramData;
    private readonly ConcurrentDictionary<string, List<TimeSpan>> _timerData;
    private readonly object _lock = new();
    private volatile bool _disposed;

    /// <summary>
    /// Gets the metrics collector name
    /// </summary>
    public string Name => "RabbitMQ Metrics Collector";

    /// <summary>
    /// Gets a value indicating whether the metrics collector is enabled
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Initializes a new instance of the RabbitMQMetricsCollector class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public RabbitMQMetricsCollector(ILogger<RabbitMQMetricsCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _counters = new ConcurrentDictionary<string, CounterMetric>();
        _gauges = new ConcurrentDictionary<string, GaugeMetric>();
        _histogramData = new ConcurrentDictionary<string, List<double>>();
        _timerData = new ConcurrentDictionary<string, List<TimeSpan>>();
    }

    /// <summary>
    /// Records a message published metric
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="messageSize">Message size in bytes</param>
    /// <param name="duration">Publishing duration</param>
    /// <param name="isSuccessful">Whether publishing was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordMessagePublished(string exchange, string routingKey, long messageSize, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"messages_published_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordHistogram("message_publish_size_bytes", messageSize, tags);
        RecordTimer("message_publish_duration", duration, tags);
        
        _logger.LogDebug("Recorded message published metric: {Exchange}/{RoutingKey}, Size: {Size}, Duration: {Duration}ms, Success: {Success}", 
            exchange, routingKey, messageSize, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records a message consumed metric
    /// </summary>
    /// <param name="queue">Queue name</param>
    /// <param name="messageSize">Message size in bytes</param>
    /// <param name="duration">Processing duration</param>
    /// <param name="isSuccessful">Whether consumption was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordMessageConsumed(string queue, long messageSize, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"messages_consumed_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordHistogram("message_consume_size_bytes", messageSize, tags);
        RecordTimer("message_consume_duration", duration, tags);
        
        _logger.LogDebug("Recorded message consumed metric: {Queue}, Size: {Size}, Duration: {Duration}ms, Success: {Success}", 
            queue, messageSize, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records a connection metric
    /// </summary>
    /// <param name="connectionId">Connection identifier</param>
    /// <param name="connectionType">Connection type</param>
    /// <param name="duration">Connection duration</param>
    /// <param name="isSuccessful">Whether connection was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordConnection(string connectionId, string connectionType, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"connections_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordTimer("connection_duration", duration, tags);
        
        _logger.LogDebug("Recorded connection metric: {ConnectionId}, Type: {Type}, Duration: {Duration}ms, Success: {Success}", 
            connectionId, connectionType, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records a channel metric
    /// </summary>
    /// <param name="channelId">Channel identifier</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordChannelOperation(string channelId, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"channel_operations_{operation}_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordTimer($"channel_operation_{operation}_duration", duration, tags);
        
        _logger.LogDebug("Recorded channel operation metric: {ChannelId}, Operation: {Operation}, Duration: {Duration}ms, Success: {Success}", 
            channelId, operation, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records a queue operation metric
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordQueueOperation(string queueName, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"queue_operations_{operation}_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordTimer($"queue_operation_{operation}_duration", duration, tags);
        
        _logger.LogDebug("Recorded queue operation metric: {QueueName}, Operation: {Operation}, Duration: {Duration}ms, Success: {Success}", 
            queueName, operation, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records an exchange operation metric
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    public void RecordExchangeOperation(string exchangeName, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"exchange_operations_{operation}_{(isSuccessful ? "success" : "failure")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordTimer($"exchange_operation_{operation}_duration", duration, tags);
        
        _logger.LogDebug("Recorded exchange operation metric: {ExchangeName}, Operation: {Operation}, Duration: {Duration}ms, Success: {Success}", 
            exchangeName, operation, duration.TotalMilliseconds, isSuccessful);
    }

    /// <summary>
    /// Records an error metric
    /// </summary>
    /// <param name="errorType">Error type</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="component">Component where error occurred</param>
    /// <param name="tags">Additional tags</param>
    public void RecordError(string errorType, string errorMessage, string component, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"errors_{errorType}";
        IncrementCounter(metricName, tags: tags);
        
        var componentMetricName = $"errors_by_component_{component}";
        IncrementCounter(componentMetricName, tags: tags);
        
        _logger.LogDebug("Recorded error metric: {ErrorType} in {Component}: {ErrorMessage}", 
            errorType, component, errorMessage);
    }

    /// <summary>
    /// Records a retry metric
    /// </summary>
    /// <param name="operation">Operation being retried</param>
    /// <param name="attemptNumber">Attempt number</param>
    /// <param name="delay">Retry delay</param>
    /// <param name="reason">Retry reason</param>
    /// <param name="tags">Additional tags</param>
    public void RecordRetry(string operation, int attemptNumber, TimeSpan delay, string reason, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"retries_{operation}";
        IncrementCounter(metricName, tags: tags);
        
        RecordGauge($"retry_attempt_number_{operation}", attemptNumber, tags);
        RecordTimer($"retry_delay_{operation}", delay, tags);
        
        _logger.LogDebug("Recorded retry metric: {Operation}, Attempt: {AttemptNumber}, Delay: {Delay}ms, Reason: {Reason}", 
            operation, attemptNumber, delay.TotalMilliseconds, reason);
    }

    /// <summary>
    /// Records a dead letter queue metric
    /// </summary>
    /// <param name="queue">Original queue name</param>
    /// <param name="deadLetterQueue">Dead letter queue name</param>
    /// <param name="reason">Reason for dead letter</param>
    /// <param name="attemptNumber">Number of attempts made</param>
    /// <param name="tags">Additional tags</param>
    public void RecordDeadLetter(string queue, string deadLetterQueue, string reason, int attemptNumber, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = "dead_letter_messages";
        IncrementCounter(metricName, tags: tags);
        
        RecordGauge($"dead_letter_attempts_{queue}", attemptNumber, tags);
        
        _logger.LogDebug("Recorded dead letter metric: {Queue} -> {DeadLetterQueue}, Attempts: {AttemptNumber}, Reason: {Reason}", 
            queue, deadLetterQueue, attemptNumber, reason);
    }

    /// <summary>
    /// Records a health check metric
    /// </summary>
    /// <param name="component">Component being checked</param>
    /// <param name="isHealthy">Whether component is healthy</param>
    /// <param name="duration">Health check duration</param>
    /// <param name="tags">Additional tags</param>
    public void RecordHealthCheck(string component, bool isHealthy, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var metricName = $"health_check_{component}_{(isHealthy ? "healthy" : "unhealthy")}";
        IncrementCounter(metricName, tags: tags);
        
        RecordTimer($"health_check_{component}_duration", duration, tags);
        RecordGauge($"health_status_{component}", isHealthy ? 1 : 0, tags);
        
        _logger.LogDebug("Recorded health check metric: {Component}, Healthy: {IsHealthy}, Duration: {Duration}ms", 
            component, isHealthy, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="metricName">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="unit">Metric unit</param>
    /// <param name="tags">Additional tags</param>
    public void RecordCustomMetric(string metricName, double value, string? unit = null, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        RecordGauge(metricName, value, tags);
        
        _logger.LogDebug("Recorded custom metric: {MetricName} = {Value} {Unit}", 
            metricName, value, unit ?? "");
    }

    /// <summary>
    /// Increments a counter metric
    /// </summary>
    /// <param name="counterName">Counter name</param>
    /// <param name="value">Value to increment by</param>
    /// <param name="tags">Additional tags</param>
    public void IncrementCounter(string counterName, long value = 1, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var key = GenerateMetricKey(counterName, tags);
        _counters.AddOrUpdate(key, new CounterMetric(counterName, value, tags), 
            (_, existing) => new CounterMetric(counterName, existing.Value + value, tags));
    }

    /// <summary>
    /// Records a gauge metric
    /// </summary>
    /// <param name="gaugeName">Gauge name</param>
    /// <param name="value">Gauge value</param>
    /// <param name="tags">Additional tags</param>
    public void RecordGauge(string gaugeName, double value, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var key = GenerateMetricKey(gaugeName, tags);
        _gauges.AddOrUpdate(key, new GaugeMetric(gaugeName, value, tags), 
            (_, _) => new GaugeMetric(gaugeName, value, tags));
    }

    /// <summary>
    /// Records a histogram metric
    /// </summary>
    /// <param name="histogramName">Histogram name</param>
    /// <param name="value">Value to record</param>
    /// <param name="tags">Additional tags</param>
    public void RecordHistogram(string histogramName, double value, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var key = GenerateMetricKey(histogramName, tags);
        _histogramData.AddOrUpdate(key, new List<double> { value }, (_, existing) => 
        {
            lock (existing)
            {
                existing.Add(value);
                return existing;
            }
        });
    }

    /// <summary>
    /// Records a timer metric
    /// </summary>
    /// <param name="timerName">Timer name</param>
    /// <param name="duration">Duration to record</param>
    /// <param name="tags">Additional tags</param>
    public void RecordTimer(string timerName, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        if (!IsEnabled) return;

        var key = GenerateMetricKey(timerName, tags);
        _timerData.AddOrUpdate(key, new List<TimeSpan> { duration }, (_, existing) => 
        {
            lock (existing)
            {
                existing.Add(duration);
                return existing;
            }
        });
    }

    /// <summary>
    /// Starts a timer for an operation
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="tags">Additional tags</param>
    /// <returns>Timer context</returns>
    public IMetricsTimer StartTimer(string operation, Dictionary<string, string>? tags = null)
    {
        return new RabbitMQMetricsTimer(this, operation, tags);
    }

    /// <summary>
    /// Gets current metrics snapshot
    /// </summary>
    /// <returns>Metrics snapshot</returns>
    public MetricsSnapshot GetSnapshot()
    {
        var counters = new Dictionary<string, CounterMetric>(_counters);
        var gauges = new Dictionary<string, GaugeMetric>(_gauges);
        var histograms = new Dictionary<string, HistogramMetric>();
        var timers = new Dictionary<string, TimerMetric>();

        // Generate histogram metrics
        foreach (var kvp in _histogramData)
        {
            var values = kvp.Value;
            if (values.Count > 0)
            {
                lock (values)
                {
                    var sortedValues = values.OrderBy(v => v).ToList();
                    var count = sortedValues.Count;
                    var sum = sortedValues.Sum();
                    var min = sortedValues.Min();
                    var max = sortedValues.Max();
                    var mean = sum / count;
                    var standardDeviation = Math.Sqrt(sortedValues.Sum(v => Math.Pow(v - mean, 2)) / count);
                    
                    var percentiles = new Dictionary<double, double>
                    {
                        [0.5] = GetPercentile(sortedValues, 0.5),
                        [0.75] = GetPercentile(sortedValues, 0.75),
                        [0.95] = GetPercentile(sortedValues, 0.95),
                        [0.99] = GetPercentile(sortedValues, 0.99)
                    };

                    histograms[kvp.Key] = new HistogramMetric(kvp.Key, count, sum, min, max, mean, standardDeviation, percentiles);
                }
            }
        }

        // Generate timer metrics
        foreach (var kvp in _timerData)
        {
            var durations = kvp.Value;
            if (durations.Count > 0)
            {
                lock (durations)
                {
                    var sortedDurations = durations.OrderBy(d => d).ToList();
                    var count = sortedDurations.Count;
                    var totalDuration = TimeSpan.FromTicks(sortedDurations.Sum(d => d.Ticks));
                    var minDuration = sortedDurations.Min();
                    var maxDuration = sortedDurations.Max();
                    var meanDuration = TimeSpan.FromTicks(totalDuration.Ticks / count);
                    
                    var durationPercentiles = new Dictionary<double, TimeSpan>
                    {
                        [0.5] = sortedDurations[(int)(count * 0.5)],
                        [0.75] = sortedDurations[(int)(count * 0.75)],
                        [0.95] = sortedDurations[(int)(count * 0.95)],
                        [0.99] = sortedDurations[(int)(count * 0.99)]
                    };

                    timers[kvp.Key] = new TimerMetric(kvp.Key, count, totalDuration, minDuration, maxDuration, meanDuration, durationPercentiles);
                }
            }
        }

        return new MetricsSnapshot(counters, gauges, histograms, timers);
    }

    /// <summary>
    /// Resets all metrics
    /// </summary>
    public void Reset()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _counters.Clear();
            _gauges.Clear();
            _histogramData.Clear();
            _timerData.Clear();
            
            _logger.LogInformation("Metrics reset");
        }
    }

    /// <summary>
    /// Flushes metrics to the underlying storage
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the flush operation</returns>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        // In a real implementation, this would flush metrics to external storage
        // For now, we'll just log the current snapshot
        var snapshot = GetSnapshot();
        
        _logger.LogInformation("Metrics snapshot: {CounterCount} counters, {GaugeCount} gauges, {HistogramCount} histograms, {TimerCount} timers",
            snapshot.Counters.Count, snapshot.Gauges.Count, snapshot.Histograms.Count, snapshot.Timers.Count);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Enables the metrics collector
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        _logger.LogInformation("Metrics collector enabled");
    }

    /// <summary>
    /// Disables the metrics collector
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        _logger.LogInformation("Metrics collector disabled");
    }

    /// <summary>
    /// Generates a metric key including tags
    /// </summary>
    /// <param name="metricName">Metric name</param>
    /// <param name="tags">Tags</param>
    /// <returns>Metric key</returns>
    private string GenerateMetricKey(string metricName, Dictionary<string, string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return metricName;

        var tagString = string.Join(",", tags.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{metricName}[{tagString}]";
    }

    /// <summary>
    /// Gets percentile value from sorted list
    /// </summary>
    /// <param name="sortedValues">Sorted values</param>
    /// <param name="percentile">Percentile (0.0 to 1.0)</param>
    /// <returns>Percentile value</returns>
    private double GetPercentile(List<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return 0;
        
        var index = (int)(sortedValues.Count * percentile);
        if (index >= sortedValues.Count) index = sortedValues.Count - 1;
        
        return sortedValues[index];
    }

    /// <summary>
    /// Disposes the metrics collector
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            if (_disposed) return;

            _disposed = true;
            _counters.Clear();
            _gauges.Clear();
            _histogramData.Clear();
            _timerData.Clear();
        }
    }
}

/// <summary>
/// RabbitMQ metrics timer implementation
/// </summary>
public class RabbitMQMetricsTimer : IMetricsTimer
{
    private readonly RabbitMQMetricsCollector _collector;
    private readonly Stopwatch _stopwatch;
    private volatile bool _disposed;

    /// <summary>
    /// Operation name
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Timer tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; }

    /// <summary>
    /// Start time
    /// </summary>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the elapsed time
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Initializes a new instance of the RabbitMQMetricsTimer class
    /// </summary>
    /// <param name="collector">Metrics collector</param>
    /// <param name="operation">Operation name</param>
    /// <param name="tags">Tags</param>
    public RabbitMQMetricsTimer(RabbitMQMetricsCollector collector, string operation, Dictionary<string, string>? tags)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Tags = tags;
        StartTime = DateTimeOffset.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops the timer and records the metric
    /// </summary>
    public void Stop()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        _collector.RecordTimer(Operation, Elapsed, Tags);
    }

    /// <summary>
    /// Disposes the timer
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        Stop();
    }
} 