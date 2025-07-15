using FS.StreamFlow.Core.Features.Messaging.Models;

namespace FS.StreamFlow.Core.Features.Messaging.Interfaces;

/// <summary>
/// Interface for metrics collection providing comprehensive monitoring of messaging operations, performance tracking, and observability
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Gets the metrics collector name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets a value indicating whether the metrics collector is enabled
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// Records a message published metric
    /// </summary>
    /// <param name="exchange">Exchange name</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="messageSize">Message size in bytes</param>
    /// <param name="duration">Publishing duration</param>
    /// <param name="isSuccessful">Whether publishing was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordMessagePublished(string exchange, string routingKey, long messageSize, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a message consumed metric
    /// </summary>
    /// <param name="queue">Queue name</param>
    /// <param name="messageSize">Message size in bytes</param>
    /// <param name="duration">Processing duration</param>
    /// <param name="isSuccessful">Whether consumption was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordMessageConsumed(string queue, long messageSize, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a connection metric
    /// </summary>
    /// <param name="connectionId">Connection identifier</param>
    /// <param name="connectionType">Connection type</param>
    /// <param name="duration">Connection duration</param>
    /// <param name="isSuccessful">Whether connection was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordConnection(string connectionId, string connectionType, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a channel metric
    /// </summary>
    /// <param name="channelId">Channel identifier</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordChannelOperation(string channelId, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a queue operation metric
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordQueueOperation(string queueName, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records an exchange operation metric
    /// </summary>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="operation">Operation type</param>
    /// <param name="duration">Operation duration</param>
    /// <param name="isSuccessful">Whether operation was successful</param>
    /// <param name="tags">Additional tags</param>
    void RecordExchangeOperation(string exchangeName, string operation, TimeSpan duration, bool isSuccessful, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records an error metric
    /// </summary>
    /// <param name="errorType">Error type</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="component">Component where error occurred</param>
    /// <param name="tags">Additional tags</param>
    void RecordError(string errorType, string errorMessage, string component, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a retry metric
    /// </summary>
    /// <param name="operation">Operation being retried</param>
    /// <param name="attemptNumber">Attempt number</param>
    /// <param name="delay">Retry delay</param>
    /// <param name="reason">Retry reason</param>
    /// <param name="tags">Additional tags</param>
    void RecordRetry(string operation, int attemptNumber, TimeSpan delay, string reason, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a dead letter queue metric
    /// </summary>
    /// <param name="queue">Original queue name</param>
    /// <param name="deadLetterQueue">Dead letter queue name</param>
    /// <param name="reason">Reason for dead letter</param>
    /// <param name="attemptNumber">Number of attempts made</param>
    /// <param name="tags">Additional tags</param>
    void RecordDeadLetter(string queue, string deadLetterQueue, string reason, int attemptNumber, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a health check metric
    /// </summary>
    /// <param name="component">Component being checked</param>
    /// <param name="isHealthy">Whether component is healthy</param>
    /// <param name="duration">Health check duration</param>
    /// <param name="tags">Additional tags</param>
    void RecordHealthCheck(string component, bool isHealthy, TimeSpan duration, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a custom metric
    /// </summary>
    /// <param name="metricName">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="unit">Metric unit</param>
    /// <param name="tags">Additional tags</param>
    void RecordCustomMetric(string metricName, double value, string? unit = null, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Increments a counter metric
    /// </summary>
    /// <param name="counterName">Counter name</param>
    /// <param name="value">Value to increment by</param>
    /// <param name="tags">Additional tags</param>
    void IncrementCounter(string counterName, long value = 1, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a gauge metric
    /// </summary>
    /// <param name="gaugeName">Gauge name</param>
    /// <param name="value">Gauge value</param>
    /// <param name="tags">Additional tags</param>
    void RecordGauge(string gaugeName, double value, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a histogram metric
    /// </summary>
    /// <param name="histogramName">Histogram name</param>
    /// <param name="value">Value to record</param>
    /// <param name="tags">Additional tags</param>
    void RecordHistogram(string histogramName, double value, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Records a timer metric
    /// </summary>
    /// <param name="timerName">Timer name</param>
    /// <param name="duration">Duration to record</param>
    /// <param name="tags">Additional tags</param>
    void RecordTimer(string timerName, TimeSpan duration, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Starts a timer for an operation
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="tags">Additional tags</param>
    /// <returns>Timer context</returns>
    IMetricsTimer StartTimer(string operation, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Gets current metrics snapshot
    /// </summary>
    /// <returns>Metrics snapshot</returns>
    MetricsSnapshot GetSnapshot();
    
    /// <summary>
    /// Resets all metrics
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Flushes metrics to the underlying storage
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the flush operation</returns>
    Task FlushAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enables the metrics collector
    /// </summary>
    void Enable();
    
    /// <summary>
    /// Disables the metrics collector
    /// </summary>
    void Disable();
}

/// <summary>
/// Interface for metrics timer
/// </summary>
public interface IMetricsTimer : IDisposable
{
    /// <summary>
    /// Operation name
    /// </summary>
    string Operation { get; }
    
    /// <summary>
    /// Timer tags
    /// </summary>
    Dictionary<string, string>? Tags { get; }
    
    /// <summary>
    /// Start time
    /// </summary>
    DateTimeOffset StartTime { get; }
    
    /// <summary>
    /// Gets the elapsed time
    /// </summary>
    TimeSpan Elapsed { get; }
    
    /// <summary>
    /// Stops the timer and records the metric
    /// </summary>
    void Stop();
}

/// <summary>
/// Represents a snapshot of metrics at a point in time
/// </summary>
public class MetricsSnapshot
{
    /// <summary>
    /// Snapshot timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    
    /// <summary>
    /// Counter metrics
    /// </summary>
    public Dictionary<string, CounterMetric> Counters { get; }
    
    /// <summary>
    /// Gauge metrics
    /// </summary>
    public Dictionary<string, GaugeMetric> Gauges { get; }
    
    /// <summary>
    /// Histogram metrics
    /// </summary>
    public Dictionary<string, HistogramMetric> Histograms { get; }
    
    /// <summary>
    /// Timer metrics
    /// </summary>
    public Dictionary<string, TimerMetric> Timers { get; }
    
    /// <summary>
    /// Initializes a new instance of the MetricsSnapshot class
    /// </summary>
    /// <param name="counters">Counter metrics</param>
    /// <param name="gauges">Gauge metrics</param>
    /// <param name="histograms">Histogram metrics</param>
    /// <param name="timers">Timer metrics</param>
    public MetricsSnapshot(
        Dictionary<string, CounterMetric> counters,
        Dictionary<string, GaugeMetric> gauges,
        Dictionary<string, HistogramMetric> histograms,
        Dictionary<string, TimerMetric> timers)
    {
        Timestamp = DateTimeOffset.UtcNow;
        Counters = counters ?? new Dictionary<string, CounterMetric>();
        Gauges = gauges ?? new Dictionary<string, GaugeMetric>();
        Histograms = histograms ?? new Dictionary<string, HistogramMetric>();
        Timers = timers ?? new Dictionary<string, TimerMetric>();
    }
}

/// <summary>
/// Represents a counter metric
/// </summary>
public class CounterMetric
{
    /// <summary>
    /// Counter name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Counter value
    /// </summary>
    public long Value { get; }
    
    /// <summary>
    /// Counter tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset LastUpdated { get; }
    
    /// <summary>
    /// Initializes a new instance of the CounterMetric class
    /// </summary>
    /// <param name="name">Counter name</param>
    /// <param name="value">Counter value</param>
    /// <param name="tags">Counter tags</param>
    public CounterMetric(string name, long value, Dictionary<string, string>? tags = null)
    {
        Name = name;
        Value = value;
        Tags = tags;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Represents a gauge metric
/// </summary>
public class GaugeMetric
{
    /// <summary>
    /// Gauge name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gauge value
    /// </summary>
    public double Value { get; }
    
    /// <summary>
    /// Gauge tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset LastUpdated { get; }
    
    /// <summary>
    /// Initializes a new instance of the GaugeMetric class
    /// </summary>
    /// <param name="name">Gauge name</param>
    /// <param name="value">Gauge value</param>
    /// <param name="tags">Gauge tags</param>
    public GaugeMetric(string name, double value, Dictionary<string, string>? tags = null)
    {
        Name = name;
        Value = value;
        Tags = tags;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Represents a histogram metric
/// </summary>
public class HistogramMetric
{
    /// <summary>
    /// Histogram name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Sample count
    /// </summary>
    public long Count { get; }
    
    /// <summary>
    /// Sum of all values
    /// </summary>
    public double Sum { get; }
    
    /// <summary>
    /// Minimum value
    /// </summary>
    public double Min { get; }
    
    /// <summary>
    /// Maximum value
    /// </summary>
    public double Max { get; }
    
    /// <summary>
    /// Mean value
    /// </summary>
    public double Mean { get; }
    
    /// <summary>
    /// Standard deviation
    /// </summary>
    public double StandardDeviation { get; }
    
    /// <summary>
    /// Percentile values
    /// </summary>
    public Dictionary<double, double> Percentiles { get; }
    
    /// <summary>
    /// Histogram tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset LastUpdated { get; }
    
    /// <summary>
    /// Initializes a new instance of the HistogramMetric class
    /// </summary>
    /// <param name="name">Histogram name</param>
    /// <param name="count">Sample count</param>
    /// <param name="sum">Sum of values</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="mean">Mean value</param>
    /// <param name="standardDeviation">Standard deviation</param>
    /// <param name="percentiles">Percentile values</param>
    /// <param name="tags">Histogram tags</param>
    public HistogramMetric(
        string name,
        long count,
        double sum,
        double min,
        double max,
        double mean,
        double standardDeviation,
        Dictionary<double, double> percentiles,
        Dictionary<string, string>? tags = null)
    {
        Name = name;
        Count = count;
        Sum = sum;
        Min = min;
        Max = max;
        Mean = mean;
        StandardDeviation = standardDeviation;
        Percentiles = percentiles;
        Tags = tags;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Represents a timer metric
/// </summary>
public class TimerMetric
{
    /// <summary>
    /// Timer name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Sample count
    /// </summary>
    public long Count { get; }
    
    /// <summary>
    /// Total duration
    /// </summary>
    public TimeSpan TotalDuration { get; }
    
    /// <summary>
    /// Minimum duration
    /// </summary>
    public TimeSpan MinDuration { get; }
    
    /// <summary>
    /// Maximum duration
    /// </summary>
    public TimeSpan MaxDuration { get; }
    
    /// <summary>
    /// Mean duration
    /// </summary>
    public TimeSpan MeanDuration { get; }
    
    /// <summary>
    /// Duration percentiles
    /// </summary>
    public Dictionary<double, TimeSpan> DurationPercentiles { get; }
    
    /// <summary>
    /// Timer tags
    /// </summary>
    public Dictionary<string, string>? Tags { get; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset LastUpdated { get; }
    
    /// <summary>
    /// Initializes a new instance of the TimerMetric class
    /// </summary>
    /// <param name="name">Timer name</param>
    /// <param name="count">Sample count</param>
    /// <param name="totalDuration">Total duration</param>
    /// <param name="minDuration">Minimum duration</param>
    /// <param name="maxDuration">Maximum duration</param>
    /// <param name="meanDuration">Mean duration</param>
    /// <param name="durationPercentiles">Duration percentiles</param>
    /// <param name="tags">Timer tags</param>
    public TimerMetric(
        string name,
        long count,
        TimeSpan totalDuration,
        TimeSpan minDuration,
        TimeSpan maxDuration,
        TimeSpan meanDuration,
        Dictionary<double, TimeSpan> durationPercentiles,
        Dictionary<string, string>? tags = null)
    {
        Name = name;
        Count = count;
        TotalDuration = totalDuration;
        MinDuration = minDuration;
        MaxDuration = maxDuration;
        MeanDuration = meanDuration;
        DurationPercentiles = durationPercentiles;
        Tags = tags;
        LastUpdated = DateTimeOffset.UtcNow;
    }
} 