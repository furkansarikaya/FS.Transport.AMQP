using FS.Mediator.Features.RequestHandling.Core;
using FS.RabbitMQ.Producer;

namespace FS.RabbitMQ.Consumer;

/// <summary>
/// Query to get consumer status
/// </summary>
public class GetConsumerStatusQuery : IRequest<ConsumerStatusQueryResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to include detailed information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get consumer status query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="includeDetails">Whether to include detailed information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer status query</returns>
    public static GetConsumerStatusQuery Create(string? consumerTag = null, bool includeDetails = false, 
        string? correlationId = null)
    {
        return new GetConsumerStatusQuery
        {
            ConsumerTag = consumerTag,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer metrics
/// </summary>
public class GetConsumerMetricsQuery : IRequest<ConsumerMetricsResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
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
    /// Creates a get consumer metrics query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="aggregation">Aggregation level</param>
    /// <param name="includePercentiles">Include percentiles</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer metrics query</returns>
    public static GetConsumerMetricsQuery Create(string? consumerTag = null, DateTimeOffset? startTime = null, 
        DateTimeOffset? endTime = null, MetricsAggregation aggregation = MetricsAggregation.Summary, 
        bool includePercentiles = false, string? correlationId = null)
    {
        return new GetConsumerMetricsQuery
        {
            ConsumerTag = consumerTag,
            StartTime = startTime,
            EndTime = endTime,
            Aggregation = aggregation,
            IncludePercentiles = includePercentiles,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer statistics
/// </summary>
public class GetConsumerStatisticsQuery : IRequest<ConsumerStatisticsQueryResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
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
    /// Creates a get consumer statistics query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="resetAfterRead">Reset statistics after read</param>
    /// <param name="category">Statistics category</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer statistics query</returns>
    public static GetConsumerStatisticsQuery Create(string? consumerTag = null, bool resetAfterRead = false, 
        StatisticsCategory? category = null, string? correlationId = null)
    {
        return new GetConsumerStatisticsQuery
        {
            ConsumerTag = consumerTag,
            ResetAfterRead = resetAfterRead,
            Category = category,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get message count from a queue
/// </summary>
public class GetMessageCountQuery : IRequest<MessageCountResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include consumer count
    /// </summary>
    public bool IncludeConsumerCount { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get message count query
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="includeConsumerCount">Include consumer count</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get message count query</returns>
    public static GetMessageCountQuery Create(string queueName, bool includeConsumerCount = false, 
        string? correlationId = null)
    {
        return new GetMessageCountQuery
        {
            QueueName = queueName,
            IncludeConsumerCount = includeConsumerCount,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer count for a queue
/// </summary>
public class GetConsumerCountQuery : IRequest<ConsumerCountResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include consumer details
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get consumer count query
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="includeDetails">Include consumer details</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer count query</returns>
    public static GetConsumerCountQuery Create(string queueName, bool includeDetails = false, 
        string? correlationId = null)
    {
        return new GetConsumerCountQuery
        {
            QueueName = queueName,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer performance metrics
/// </summary>
public class GetConsumerPerformanceQuery : IRequest<ConsumerPerformanceResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
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
    /// Creates a get consumer performance query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="timeWindow">Time window</param>
    /// <param name="metricsToInclude">Metrics to include</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer performance query</returns>
    public static GetConsumerPerformanceQuery Create(string? consumerTag = null, TimeSpan? timeWindow = null, 
        PerformanceMetrics metricsToInclude = PerformanceMetrics.All, string? correlationId = null)
    {
        return new GetConsumerPerformanceQuery
        {
            ConsumerTag = consumerTag,
            TimeWindow = timeWindow ?? TimeSpan.FromMinutes(5),
            MetricsToInclude = metricsToInclude,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer configuration
/// </summary>
public class GetConsumerConfigurationQuery : IRequest<ConsumerConfigurationResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to include sensitive information
    /// </summary>
    public bool IncludeSensitive { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get consumer configuration query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="includeSensitive">Include sensitive information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer configuration query</returns>
    public static GetConsumerConfigurationQuery Create(string? consumerTag = null, bool includeSensitive = false, 
        string? correlationId = null)
    {
        return new GetConsumerConfigurationQuery
        {
            ConsumerTag = consumerTag,
            IncludeSensitive = includeSensitive,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer health status
/// </summary>
public class GetConsumerHealthQuery : IRequest<ConsumerHealthResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Whether to include detailed health information
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get consumer health query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="includeDetails">Include detailed information</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer health query</returns>
    public static GetConsumerHealthQuery Create(string? consumerTag = null, bool includeDetails = false, 
        string? correlationId = null)
    {
        return new GetConsumerHealthQuery
        {
            ConsumerTag = consumerTag,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get active consumers
/// </summary>
public class GetActiveConsumersQuery : IRequest<ActiveConsumersResult>
{
    /// <summary>
    /// Queue name filter
    /// </summary>
    public string? QueueName { get; set; }
    
    /// <summary>
    /// Whether to include consumer details
    /// </summary>
    public bool IncludeDetails { get; set; }
    
    /// <summary>
    /// Maximum number of consumers to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get active consumers query
    /// </summary>
    /// <param name="queueName">Queue name filter</param>
    /// <param name="includeDetails">Include consumer details</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get active consumers query</returns>
    public static GetActiveConsumersQuery Create(string? queueName = null, bool includeDetails = false, 
        int? maxResults = null, string? correlationId = null)
    {
        return new GetActiveConsumersQuery
        {
            QueueName = queueName,
            IncludeDetails = includeDetails,
            MaxResults = maxResults,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get consumer processing history
/// </summary>
public class GetConsumerHistoryQuery : IRequest<ConsumerHistoryResult>
{
    /// <summary>
    /// Consumer tag filter
    /// </summary>
    public string? ConsumerTag { get; set; }
    
    /// <summary>
    /// Start time for history period
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }
    
    /// <summary>
    /// End time for history period
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Maximum number of history entries to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Whether to include message details
    /// </summary>
    public bool IncludeMessageDetails { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a get consumer history query
    /// </summary>
    /// <param name="consumerTag">Consumer tag filter</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="includeMessageDetails">Include message details</param>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Get consumer history query</returns>
    public static GetConsumerHistoryQuery Create(string? consumerTag = null, DateTimeOffset? startTime = null, 
        DateTimeOffset? endTime = null, int? maxResults = null, bool includeMessageDetails = false, 
        string? correlationId = null)
    {
        return new GetConsumerHistoryQuery
        {
            ConsumerTag = consumerTag,
            StartTime = startTime,
            EndTime = endTime,
            MaxResults = maxResults,
            IncludeMessageDetails = includeMessageDetails,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums (already defined in Producer/Queries.cs)

// Result models

/// <summary>
/// Result of consumer status query
/// </summary>
public class ConsumerStatusQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer status information
    /// </summary>
    public ConsumerStatusInfo? StatusInfo { get; set; }
    
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
    /// <param name="statusInfo">Status information</param>
    /// <returns>Success result</returns>
    public static ConsumerStatusQueryResult CreateSuccess(ConsumerStatusInfo statusInfo)
    {
        return new ConsumerStatusQueryResult
        {
            Success = true,
            StatusInfo = statusInfo
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConsumerStatusQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerStatusQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer metrics query
/// </summary>
public class ConsumerMetricsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer metrics
    /// </summary>
    public ConsumerMetrics? Metrics { get; set; }
    
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
    /// <param name="metrics">Consumer metrics</param>
    /// <param name="period">Time period</param>
    /// <returns>Success result</returns>
    public static ConsumerMetricsResult CreateSuccess(ConsumerMetrics metrics, TimeSpan period)
    {
        return new ConsumerMetricsResult
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
    public static ConsumerMetricsResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerMetricsResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer statistics query
/// </summary>
public class ConsumerStatisticsQueryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer statistics
    /// </summary>
    public ConsumerStatistics? Statistics { get; set; }
    
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
    /// <param name="statistics">Consumer statistics</param>
    /// <param name="category">Statistics category</param>
    /// <returns>Success result</returns>
    public static ConsumerStatisticsQueryResult CreateSuccess(ConsumerStatistics statistics, StatisticsCategory category)
    {
        return new ConsumerStatisticsQueryResult
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
    public static ConsumerStatisticsQueryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerStatisticsQueryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of message count query
/// </summary>
public class MessageCountResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages in queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Number of consumers for the queue
    /// </summary>
    public uint ConsumerCount { get; set; }
    
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
    /// <param name="queueName">Queue name</param>
    /// <param name="messageCount">Message count</param>
    /// <param name="consumerCount">Consumer count</param>
    /// <returns>Success result</returns>
    public static MessageCountResult CreateSuccess(string queueName, uint messageCount, uint consumerCount = 0)
    {
        return new MessageCountResult
        {
            Success = true,
            QueueName = queueName,
            MessageCount = messageCount,
            ConsumerCount = consumerCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static MessageCountResult CreateFailure(string queueName, string errorMessage, Exception? exception = null)
    {
        return new MessageCountResult
        {
            Success = false,
            QueueName = queueName,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer count query
/// </summary>
public class ConsumerCountResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of active consumers
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// Consumer details if requested
    /// </summary>
    public IEnumerable<ConsumerDetail> ConsumerDetails { get; set; } = Enumerable.Empty<ConsumerDetail>();
    
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
    /// <param name="queueName">Queue name</param>
    /// <param name="consumerCount">Consumer count</param>
    /// <param name="consumerDetails">Consumer details</param>
    /// <returns>Success result</returns>
    public static ConsumerCountResult CreateSuccess(string queueName, uint consumerCount, 
        IEnumerable<ConsumerDetail>? consumerDetails = null)
    {
        return new ConsumerCountResult
        {
            Success = true,
            QueueName = queueName,
            ConsumerCount = consumerCount,
            ConsumerDetails = consumerDetails ?? Enumerable.Empty<ConsumerDetail>()
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConsumerCountResult CreateFailure(string queueName, string errorMessage, Exception? exception = null)
    {
        return new ConsumerCountResult
        {
            Success = false,
            QueueName = queueName,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer performance query
/// </summary>
public class ConsumerPerformanceResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Performance data
    /// </summary>
    public ConsumerPerformanceData? Performance { get; set; }
    
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
    public static ConsumerPerformanceResult CreateSuccess(ConsumerPerformanceData performance, TimeSpan timeWindow)
    {
        return new ConsumerPerformanceResult
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
    public static ConsumerPerformanceResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerPerformanceResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer configuration query
/// </summary>
public class ConsumerConfigurationResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer settings
    /// </summary>
    public ConsumerSettings? Settings { get; set; }
    
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
    /// <param name="settings">Consumer settings</param>
    /// <param name="metadata">Configuration metadata</param>
    /// <returns>Success result</returns>
    public static ConsumerConfigurationResult CreateSuccess(ConsumerSettings settings, Dictionary<string, object>? metadata = null)
    {
        return new ConsumerConfigurationResult
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
    public static ConsumerConfigurationResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerConfigurationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer health query
/// </summary>
public class ConsumerHealthResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer health status
    /// </summary>
    public ConsumerHealthStatus? HealthStatus { get; set; }
    
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
    public static ConsumerHealthResult CreateSuccess(ConsumerHealthStatus healthStatus, Dictionary<string, object>? healthDetails = null)
    {
        return new ConsumerHealthResult
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
    public static ConsumerHealthResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerHealthResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of active consumers query
/// </summary>
public class ActiveConsumersResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Active consumers
    /// </summary>
    public IEnumerable<ActiveConsumerInfo> ActiveConsumers { get; set; } = Enumerable.Empty<ActiveConsumerInfo>();
    
    /// <summary>
    /// Total count of active consumers
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
    /// <param name="activeConsumers">Active consumers</param>
    /// <param name="totalCount">Total count</param>
    /// <returns>Success result</returns>
    public static ActiveConsumersResult CreateSuccess(IEnumerable<ActiveConsumerInfo> activeConsumers, int totalCount)
    {
        return new ActiveConsumersResult
        {
            Success = true,
            ActiveConsumers = activeConsumers,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ActiveConsumersResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ActiveConsumersResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of consumer history query
/// </summary>
public class ConsumerHistoryResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer history entries
    /// </summary>
    public IEnumerable<ConsumerHistoryEntry> HistoryEntries { get; set; } = Enumerable.Empty<ConsumerHistoryEntry>();
    
    /// <summary>
    /// Total count of history entries
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
    /// <param name="historyEntries">History entries</param>
    /// <param name="totalCount">Total count</param>
    /// <returns>Success result</returns>
    public static ConsumerHistoryResult CreateSuccess(IEnumerable<ConsumerHistoryEntry> historyEntries, int totalCount)
    {
        return new ConsumerHistoryResult
        {
            Success = true,
            HistoryEntries = historyEntries,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="exception">Exception</param>
    /// <returns>Failure result</returns>
    public static ConsumerHistoryResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new ConsumerHistoryResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

// Supporting data models

/// <summary>
/// Consumer status information
/// </summary>
public class ConsumerStatusInfo
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status
    /// </summary>
    public ConsumerStatus Status { get; set; }
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Last activity time
    /// </summary>
    public DateTimeOffset LastActivityTime { get; set; }
    
    /// <summary>
    /// Uptime
    /// </summary>
    public TimeSpan Uptime => DateTimeOffset.UtcNow - StartTime;
    
    /// <summary>
    /// Consumer settings
    /// </summary>
    public ConsumerSettings? Settings { get; set; }
}

/// <summary>
/// Consumer metrics data
/// </summary>
public class ConsumerMetrics
{
    /// <summary>
    /// Messages per second
    /// </summary>
    public double MessagesPerSecond { get; set; }
    
    /// <summary>
    /// Average processing latency in milliseconds
    /// </summary>
    public double AverageLatency { get; set; }
    
    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate { get; set; }
    
    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Total messages processed
    /// </summary>
    public long TotalMessages { get; set; }
    
    /// <summary>
    /// Total failed messages
    /// </summary>
    public long FailedMessages { get; set; }
    
    /// <summary>
    /// Metrics timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Consumer detail information
/// </summary>
public class ConsumerDetail
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Channel number
    /// </summary>
    public ushort ChannelNumber { get; set; }
    
    /// <summary>
    /// Connection name
    /// </summary>
    public string ConnectionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer exclusive
    /// </summary>
    public bool Exclusive { get; set; }
    
    /// <summary>
    /// Auto acknowledge
    /// </summary>
    public bool AutoAcknowledge { get; set; }
    
    /// <summary>
    /// Prefetch count
    /// </summary>
    public ushort PrefetchCount { get; set; }
    
    /// <summary>
    /// Consumer arguments
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();
}

/// <summary>
/// Consumer performance data
/// </summary>
public class ConsumerPerformanceData
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
/// Consumer health status
/// </summary>
public class ConsumerHealthStatus
{
    /// <summary>
    /// Overall health status
    /// </summary>
    public HealthStatusLevel Status { get; set; }
    
    /// <summary>
    /// Health description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Last health check time
    /// </summary>
    public DateTimeOffset LastCheckTime { get; set; }
    
    /// <summary>
    /// Connection health
    /// </summary>
    public bool ConnectionHealthy { get; set; }
    
    /// <summary>
    /// Channel health
    /// </summary>
    public bool ChannelHealthy { get; set; }
    
    /// <summary>
    /// Queue accessible
    /// </summary>
    public bool QueueAccessible { get; set; }
    
    /// <summary>
    /// Message processing rate
    /// </summary>
    public double ProcessingRate { get; set; }
}

/// <summary>
/// Health status levels
/// </summary>
public enum HealthStatusLevel
{
    /// <summary>
    /// Unknown health status
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Healthy status
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Degraded status
    /// </summary>
    Degraded,
    
    /// <summary>
    /// Unhealthy status
    /// </summary>
    Unhealthy
}

/// <summary>
/// Active consumer information
/// </summary>
public class ActiveConsumerInfo
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Last activity time
    /// </summary>
    public DateTimeOffset LastActivityTime { get; set; }
    
    /// <summary>
    /// Messages processed
    /// </summary>
    public long MessagesProcessed { get; set; }
    
    /// <summary>
    /// Processing rate (messages per second)
    /// </summary>
    public double ProcessingRate { get; set; }
    
    /// <summary>
    /// Consumer status
    /// </summary>
    public ConsumerStatus Status { get; set; }
}

/// <summary>
/// Consumer history entry
/// </summary>
public class ConsumerHistoryEntry
{
    /// <summary>
    /// Entry ID
    /// </summary>
    public string EntryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Processing start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    
    /// <summary>
    /// Processing end time
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// Processing duration
    /// </summary>
    public TimeSpan? Duration => EndTime - StartTime;
    
    /// <summary>
    /// Processing result
    /// </summary>
    public ProcessingResult Result { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Message details if requested
    /// </summary>
    public object? MessageDetails { get; set; }
}

/// <summary>
/// Processing result enumeration
/// </summary>
public enum ProcessingResult
{
    /// <summary>
    /// Processing is in progress
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Processing completed successfully
    /// </summary>
    Success,
    
    /// <summary>
    /// Processing failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Message was rejected
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Message was requeued
    /// </summary>
    Requeued,
    
    /// <summary>
    /// Processing was cancelled
    /// </summary>
    Cancelled
} 