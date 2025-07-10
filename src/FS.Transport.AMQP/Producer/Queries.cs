using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.Producer;

/// <summary>
/// Query to get producer status
/// </summary>
public class GetProducerStatusQuery : IRequest<ProducerStatusResult>
{
    /// <summary>
    /// Whether to include detailed information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer status query
    /// </summary>
    /// <param name="includeDetails">Whether to include detailed information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer status query</returns>
    public static GetProducerStatusQuery Create(bool includeDetails = false, string? correlationId = null)
    {
        return new GetProducerStatusQuery
        {
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get producer metrics
/// </summary>
public class GetProducerMetricsQuery : IRequest<ProducerMetricsResult>
{
    /// <summary>
    /// Start time for metrics period
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }
    
    /// <summary>
    /// End time for metrics period
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Metrics aggregation level
    /// </summary>
    public MetricsAggregation Aggregation { get; set; } = MetricsAggregation.Summary;
    
    /// <summary>
    /// Whether to include percentiles
    /// </summary>
    public bool IncludePercentiles { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer metrics query
    /// </summary>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="aggregation">Aggregation level</param>
    /// <param name="includePercentiles">Include percentiles</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer metrics query</returns>
    public static GetProducerMetricsQuery Create(DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, 
        MetricsAggregation aggregation = MetricsAggregation.Summary, bool includePercentiles = false, string? correlationId = null)
    {
        return new GetProducerMetricsQuery
        {
            StartTime = startTime,
            EndTime = endTime,
            Aggregation = aggregation,
            IncludePercentiles = includePercentiles,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get producer statistics
/// </summary>
public class GetProducerStatisticsQuery : IRequest<ProducerStatisticsResult>
{
    /// <summary>
    /// Whether to reset statistics after retrieval
    /// </summary>
    public bool ResetAfterRead { get; set; }
    
    /// <summary>
    /// Statistics category filter
    /// </summary>
    public StatisticsCategory? Category { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer statistics query
    /// </summary>
    /// <param name="resetAfterRead">Reset statistics after read</param>
    /// <param name="category">Statistics category</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer statistics query</returns>
    public static GetProducerStatisticsQuery Create(bool resetAfterRead = false, 
        StatisticsCategory? category = null, string? correlationId = null)
    {
        return new GetProducerStatisticsQuery
        {
            ResetAfterRead = resetAfterRead,
            Category = category,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get pending confirmations
/// </summary>
public class GetPendingConfirmationsQuery : IRequest<PendingConfirmationsResult>
{
    /// <summary>
    /// Maximum number of confirmations to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Whether to include confirmation details
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get pending confirmations query
    /// </summary>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="includeDetails">Include details</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get pending confirmations query</returns>
    public static GetPendingConfirmationsQuery Create(int? maxResults = null, 
        bool includeDetails = false, string? correlationId = null)
    {
        return new GetPendingConfirmationsQuery
        {
            MaxResults = maxResults,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get scheduled messages
/// </summary>
public class GetScheduledMessagesQuery : IRequest<ScheduledMessagesResult>
{
    /// <summary>
    /// Start time filter
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }
    
    /// <summary>
    /// End time filter
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Maximum number of messages to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Whether to include message content
    /// </summary>
    public bool IncludeContent { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get scheduled messages query
    /// </summary>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="includeContent">Include content</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get scheduled messages query</returns>
    public static GetScheduledMessagesQuery Create(DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, 
        int? maxResults = null, bool includeContent = false, string? correlationId = null)
    {
        return new GetScheduledMessagesQuery
        {
            StartTime = startTime,
            EndTime = endTime,
            MaxResults = maxResults,
            IncludeContent = includeContent,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get producer performance metrics
/// </summary>
public class GetProducerPerformanceQuery : IRequest<ProducerPerformanceResult>
{
    /// <summary>
    /// Time window for performance data
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Performance metrics to include
    /// </summary>
    public PerformanceMetrics MetricsToInclude { get; set; } = PerformanceMetrics.All;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer performance query
    /// </summary>
    /// <param name="timeWindow">Time window</param>
    /// <param name="metricsToInclude">Metrics to include</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer performance query</returns>
    public static GetProducerPerformanceQuery Create(TimeSpan? timeWindow = null, 
        PerformanceMetrics metricsToInclude = PerformanceMetrics.All, string? correlationId = null)
    {
        return new GetProducerPerformanceQuery
        {
            TimeWindow = timeWindow ?? TimeSpan.FromMinutes(5),
            MetricsToInclude = metricsToInclude,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get producer configuration
/// </summary>
public class GetProducerConfigurationQuery : IRequest<ProducerConfigurationResult>
{
    /// <summary>
    /// Whether to include sensitive information
    /// </summary>
    public bool IncludeSensitive { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer configuration query
    /// </summary>
    /// <param name="includeSensitive">Include sensitive information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer configuration query</returns>
    public static GetProducerConfigurationQuery Create(bool includeSensitive = false, string? correlationId = null)
    {
        return new GetProducerConfigurationQuery
        {
            IncludeSensitive = includeSensitive,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get producer health status
/// </summary>
public class GetProducerHealthQuery : IRequest<ProducerHealthResult>
{
    /// <summary>
    /// Whether to include detailed health information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get producer health query
    /// </summary>
    /// <param name="includeDetails">Include detailed information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get producer health query</returns>
    public static GetProducerHealthQuery Create(bool includeDetails = false, string? correlationId = null)
    {
        return new GetProducerHealthQuery
        {
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums and models

/// <summary>
/// Metrics aggregation levels
/// </summary>
public enum MetricsAggregation
{
    /// <summary>
    /// Summary metrics only
    /// </summary>
    Summary,
    
    /// <summary>
    /// Detailed metrics
    /// </summary>
    Detailed,
    
    /// <summary>
    /// Raw metrics data
    /// </summary>
    Raw
}

/// <summary>
/// Statistics categories
/// </summary>
public enum StatisticsCategory
{
    /// <summary>
    /// All statistics
    /// </summary>
    All,
    
    /// <summary>
    /// Message statistics
    /// </summary>
    Messages,
    
    /// <summary>
    /// Performance statistics
    /// </summary>
    Performance,
    
    /// <summary>
    /// Error statistics
    /// </summary>
    Errors,
    
    /// <summary>
    /// Connection statistics
    /// </summary>
    Connection,
    
    /// <summary>
    /// Batch statistics
    /// </summary>
    Batches
}

/// <summary>
/// Performance metrics flags
/// </summary>
[Flags]
public enum PerformanceMetrics
{
    /// <summary>
    /// No metrics
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Throughput metrics
    /// </summary>
    Throughput = 1,
    
    /// <summary>
    /// Latency metrics
    /// </summary>
    Latency = 2,
    
    /// <summary>
    /// Error rate metrics
    /// </summary>
    ErrorRate = 4,
    
    /// <summary>
    /// Resource usage metrics
    /// </summary>
    ResourceUsage = 8,
    
    /// <summary>
    /// All metrics
    /// </summary>
    All = Throughput | Latency | ErrorRate | ResourceUsage
}

// Result models

/// <summary>
/// Result of producer metrics query
/// </summary>
public class ProducerMetricsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Producer metrics (using existing ProducerMetrics class from ProducerExtensions.cs)
    /// </summary>
    public ProducerMetrics? Metrics { get; set; }
    
    /// <summary>
    /// Time period for metrics
    /// </summary>
    public TimeSpan Period { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="metrics">Producer metrics</param>
    /// <param name="period">Time period</param>
    /// <returns>Success result</returns>
    public static ProducerMetricsResult CreateSuccess(ProducerMetrics metrics, TimeSpan period)
    {
        return new ProducerMetricsResult
        {
            Success = true,
            Metrics = metrics,
            Period = period
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerMetricsResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ProducerMetricsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of producer statistics query
/// </summary>
public class ProducerStatisticsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Producer statistics
    /// </summary>
    public ProducerStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Statistics category
    /// </summary>
    public StatisticsCategory Category { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="statistics">Producer statistics</param>
    /// <param name="category">Statistics category</param>
    /// <returns>Success result</returns>
    public static ProducerStatisticsResult CreateSuccess(ProducerStatistics statistics, StatisticsCategory category)
    {
        return new ProducerStatisticsResult
        {
            Success = true,
            Statistics = statistics,
            Category = category
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerStatisticsResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ProducerStatisticsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of pending confirmations query
/// </summary>
public class PendingConfirmationsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Pending confirmations
    /// </summary>
    public IEnumerable<PendingConfirmation> PendingConfirmations { get; set; } = Enumerable.Empty<PendingConfirmation>();
    
    /// <summary>
    /// Total count of pending confirmations
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="pendingConfirmations">Pending confirmations</param>
    /// <param name="totalCount">Total count</param>
    /// <returns>Success result</returns>
    public static PendingConfirmationsResult CreateSuccess(IEnumerable<PendingConfirmation> pendingConfirmations, int totalCount)
    {
        return new PendingConfirmationsResult
        {
            Success = true,
            PendingConfirmations = pendingConfirmations,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static PendingConfirmationsResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new PendingConfirmationsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of scheduled messages query
/// </summary>
public class ScheduledMessagesResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Scheduled messages
    /// </summary>
    public IEnumerable<ScheduledMessageInfo> ScheduledMessages { get; set; } = Enumerable.Empty<ScheduledMessageInfo>();
    
    /// <summary>
    /// Total count of scheduled messages
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="scheduledMessages">Scheduled messages</param>
    /// <param name="totalCount">Total count</param>
    /// <returns>Success result</returns>
    public static ScheduledMessagesResult CreateSuccess(IEnumerable<ScheduledMessageInfo> scheduledMessages, int totalCount)
    {
        return new ScheduledMessagesResult
        {
            Success = true,
            ScheduledMessages = scheduledMessages,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ScheduledMessagesResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ScheduledMessagesResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of producer performance query
/// </summary>
public class ProducerPerformanceResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Performance data
    /// </summary>
    public ProducerPerformanceData? Performance { get; set; }
    
    /// <summary>
    /// Time window for performance data
    /// </summary>
    public TimeSpan TimeWindow { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="performance">Performance data</param>
    /// <param name="timeWindow">Time window</param>
    /// <returns>Success result</returns>
    public static ProducerPerformanceResult CreateSuccess(ProducerPerformanceData performance, TimeSpan timeWindow)
    {
        return new ProducerPerformanceResult
        {
            Success = true,
            Performance = performance,
            TimeWindow = timeWindow
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerPerformanceResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ProducerPerformanceResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of producer configuration query
/// </summary>
public class ProducerConfigurationResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Producer settings
    /// </summary>
    public ProducerSettings? Settings { get; set; }
    
    /// <summary>
    /// Configuration metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="settings">Producer settings</param>
    /// <param name="metadata">Configuration metadata</param>
    /// <returns>Success result</returns>
    public static ProducerConfigurationResult CreateSuccess(ProducerSettings settings, Dictionary<string, object>? metadata = null)
    {
        return new ProducerConfigurationResult
        {
            Success = true,
            Settings = settings,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerConfigurationResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ProducerConfigurationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of producer health query
/// </summary>
public class ProducerHealthResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Producer health status (using existing ProducerHealth class from ProducerExtensions.cs)
    /// </summary>
    public ProducerHealth? HealthStatus { get; set; }
    
    /// <summary>
    /// Health check details
    /// </summary>
    public Dictionary<string, object> HealthDetails { get; set; } = new();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    /// <param name="healthStatus">Health status</param>
    /// <param name="healthDetails">Health details</param>
    /// <returns>Success result</returns>
    public static ProducerHealthResult CreateSuccess(ProducerHealth healthStatus, Dictionary<string, object>? healthDetails = null)
    {
        return new ProducerHealthResult
        {
            Success = true,
            HealthStatus = healthStatus,
            HealthDetails = healthDetails ?? new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ProducerHealthResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ProducerHealthResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

// Supporting data models

/// <summary>
/// Pending confirmation information
/// </summary>
public class PendingConfirmation
{
    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Sequence number
    /// </summary>
    public ulong SequenceNumber { get; set; }
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when message was published
    /// </summary>
    public DateTimeOffset PublishedAt { get; set; }
    
    /// <summary>
    /// Time elapsed since publication
    /// </summary>
    public TimeSpan ElapsedTime => DateTimeOffset.UtcNow - PublishedAt;
}

/// <summary>
/// Scheduled message information
/// </summary>
public class ScheduledMessageInfo
{
    /// <summary>
    /// Schedule ID
    /// </summary>
    public string ScheduleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Scheduled time
    /// </summary>
    public DateTimeOffset ScheduledTime { get; set; }
    
    /// <summary>
    /// Message content (if requested)
    /// </summary>
    public object? Content { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Producer performance data
/// </summary>
public class ProducerPerformanceData
{
    /// <summary>
    /// Throughput metrics
    /// </summary>
    public ThroughputMetrics? Throughput { get; set; }
    
    /// <summary>
    /// Latency metrics
    /// </summary>
    public LatencyMetrics? Latency { get; set; }
    
    /// <summary>
    /// Error metrics
    /// </summary>
    public ErrorMetrics? Errors { get; set; }
    
    /// <summary>
    /// Resource usage metrics
    /// </summary>
    public ResourceUsageMetrics? ResourceUsage { get; set; }
}

/// <summary>
/// Throughput metrics
/// </summary>
public class ThroughputMetrics
{
    /// <summary>
    /// Messages per second
    /// </summary>
    public double MessagesPerSecond { get; set; }
    
    /// <summary>
    /// Peak throughput
    /// </summary>
    public double PeakThroughput { get; set; }
    
    /// <summary>
    /// Average throughput
    /// </summary>
    public double AverageThroughput { get; set; }
}

/// <summary>
/// Latency metrics
/// </summary>
public class LatencyMetrics
{
    /// <summary>
    /// Average latency in milliseconds
    /// </summary>
    public double Average { get; set; }
    
    /// <summary>
    /// Minimum latency in milliseconds
    /// </summary>
    public double Minimum { get; set; }
    
    /// <summary>
    /// Maximum latency in milliseconds
    /// </summary>
    public double Maximum { get; set; }
    
    /// <summary>
    /// 95th percentile latency
    /// </summary>
    public double P95 { get; set; }
    
    /// <summary>
    /// 99th percentile latency
    /// </summary>
    public double P99 { get; set; }
}

/// <summary>
/// Error metrics
/// </summary>
public class ErrorMetrics
{
    /// <summary>
    /// Total error count
    /// </summary>
    public long TotalErrors { get; set; }
    
    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Errors by type
    /// </summary>
    public Dictionary<string, long> ErrorsByType { get; set; } = new();
}

/// <summary>
/// Resource usage metrics
/// </summary>
public class ResourceUsageMetrics
{
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsage { get; set; }
    
    /// <summary>
    /// Thread count
    /// </summary>
    public int ThreadCount { get; set; }
} 