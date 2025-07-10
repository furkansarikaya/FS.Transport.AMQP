using FS.Mediator.Features.RequestHandling.Core;

namespace FS.Transport.AMQP.Queue;

/// <summary>
/// Query to get queue information
/// </summary>
public class GetQueueInfoQuery : IRequest<QueueInfoResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include detailed information
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
    
    /// <summary>
    /// Whether to include binding information
    /// </summary>
    public bool IncludeBindings { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue info query
    /// </summary>
    public static GetQueueInfoQuery Create(string name, bool includeDetails = true, bool includeBindings = true, string? correlationId = null)
    {
        return new GetQueueInfoQuery
        {
            Name = name,
            IncludeDetails = includeDetails,
            IncludeBindings = includeBindings,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue statistics
/// </summary>
public class GetQueueStatisticsQuery : IRequest<QueueStatisticsResult>
{
    /// <summary>
    /// Queue name (optional, null for all queues)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Statistics categories to include
    /// </summary>
    public QueueStatisticsCategory Categories { get; set; } = QueueStatisticsCategory.All;
    
    /// <summary>
    /// Whether to include historical data
    /// </summary>
    public bool IncludeHistory { get; set; } = false;
    
    /// <summary>
    /// Time period for historical data
    /// </summary>
    public TimeSpan? HistoryPeriod { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue statistics query
    /// </summary>
    public static GetQueueStatisticsQuery Create(string? name = null, QueueStatisticsCategory categories = QueueStatisticsCategory.All, 
        bool includeHistory = false, TimeSpan? historyPeriod = null, string? correlationId = null)
    {
        return new GetQueueStatisticsQuery
        {
            Name = name,
            Categories = categories,
            IncludeHistory = includeHistory,
            HistoryPeriod = historyPeriod,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue message count
/// </summary>
public class GetQueueMessageCountQuery : IRequest<QueueMessageCountResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include ready messages only
    /// </summary>
    public bool ReadyOnly { get; set; } = false;
    
    /// <summary>
    /// Whether to include unacknowledged messages
    /// </summary>
    public bool IncludeUnacknowledged { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue message count query
    /// </summary>
    public static GetQueueMessageCountQuery Create(string name, bool readyOnly = false, bool includeUnacknowledged = true, string? correlationId = null)
    {
        return new GetQueueMessageCountQuery
        {
            Name = name,
            ReadyOnly = readyOnly,
            IncludeUnacknowledged = includeUnacknowledged,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue consumer count
/// </summary>
public class GetQueueConsumerCountQuery : IRequest<QueueConsumerCountResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to include consumer details
    /// </summary>
    public bool IncludeDetails { get; set; } = false;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue consumer count query
    /// </summary>
    public static GetQueueConsumerCountQuery Create(string name, bool includeDetails = false, string? correlationId = null)
    {
        return new GetQueueConsumerCountQuery
        {
            Name = name,
            IncludeDetails = includeDetails,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to check if queue exists
/// </summary>
public class QueueExistsQuery : IRequest<QueueExistsResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue exists query
    /// </summary>
    public static QueueExistsQuery Create(string name, string? correlationId = null)
    {
        return new QueueExistsQuery
        {
            Name = name,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue bindings
/// </summary>
public class GetQueueBindingsQuery : IRequest<QueueBindingsResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange name filter (optional)
    /// </summary>
    public string? ExchangeFilter { get; set; }
    
    /// <summary>
    /// Routing key filter (optional)
    /// </summary>
    public string? RoutingKeyFilter { get; set; }
    
    /// <summary>
    /// Whether to include binding arguments
    /// </summary>
    public bool IncludeArguments { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue bindings query
    /// </summary>
    public static GetQueueBindingsQuery Create(string name, string? exchangeFilter = null, string? routingKeyFilter = null, 
        bool includeArguments = true, string? correlationId = null)
    {
        return new GetQueueBindingsQuery
        {
            Name = name,
            ExchangeFilter = exchangeFilter,
            RoutingKeyFilter = routingKeyFilter,
            IncludeArguments = includeArguments,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue health status
/// </summary>
public class GetQueueHealthQuery : IRequest<QueueHealthResult>
{
    /// <summary>
    /// Queue name (optional, null for all queues)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Whether to include detailed health information
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
    
    /// <summary>
    /// Health check timeout
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue health query
    /// </summary>
    public static GetQueueHealthQuery Create(string? name = null, bool includeDetails = true, TimeSpan? timeout = null, string? correlationId = null)
    {
        return new GetQueueHealthQuery
        {
            Name = name,
            IncludeDetails = includeDetails,
            Timeout = timeout,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get queue performance metrics
/// </summary>
public class GetQueuePerformanceQuery : IRequest<QueuePerformanceResult>
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Time window for metrics
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Performance metrics to include
    /// </summary>
    public QueuePerformanceMetrics Metrics { get; set; } = QueuePerformanceMetrics.All;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a queue performance query
    /// </summary>
    public static GetQueuePerformanceQuery Create(string name, TimeSpan? timeWindow = null, 
        QueuePerformanceMetrics metrics = QueuePerformanceMetrics.All, string? correlationId = null)
    {
        return new GetQueuePerformanceQuery
        {
            Name = name,
            TimeWindow = timeWindow ?? TimeSpan.FromMinutes(5),
            Metrics = metrics,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to list all queues
/// </summary>
public class ListQueuesQuery : IRequest<QueueListResult>
{
    /// <summary>
    /// Name pattern filter (optional)
    /// </summary>
    public string? NamePattern { get; set; }
    
    /// <summary>
    /// Whether to include only durable queues
    /// </summary>
    public bool DurableOnly { get; set; } = false;
    
    /// <summary>
    /// Whether to include queue details
    /// </summary>
    public bool IncludeDetails { get; set; } = false;
    
    /// <summary>
    /// Maximum number of queues to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a list queues query
    /// </summary>
    public static ListQueuesQuery Create(string? namePattern = null, bool durableOnly = false, bool includeDetails = false, 
        int? maxResults = null, string? correlationId = null)
    {
        return new ListQueuesQuery
        {
            NamePattern = namePattern,
            DurableOnly = durableOnly,
            IncludeDetails = includeDetails,
            MaxResults = maxResults,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums

/// <summary>
/// Queue performance metrics
/// </summary>
[Flags]
public enum QueuePerformanceMetrics
{
    /// <summary>
    /// Message throughput
    /// </summary>
    Throughput = 1,
    
    /// <summary>
    /// Message latency
    /// </summary>
    Latency = 2,
    
    /// <summary>
    /// Consumer utilization
    /// </summary>
    ConsumerUtilization = 4,
    
    /// <summary>
    /// Memory usage
    /// </summary>
    MemoryUsage = 8,
    
    /// <summary>
    /// Disk usage
    /// </summary>
    DiskUsage = 16,
    
    /// <summary>
    /// All performance metrics
    /// </summary>
    All = Throughput | Latency | ConsumerUtilization | MemoryUsage | DiskUsage
}

// Result models

/// <summary>
/// Result of queue info query
/// </summary>
public class QueueInfoResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue information
    /// </summary>
    public QueueInfo? Info { get; set; }
    
    /// <summary>
    /// Queue bindings if requested
    /// </summary>
    public IEnumerable<QueueBindingInfo>? Bindings { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueInfoResult CreateSuccess(QueueInfo info, IEnumerable<QueueBindingInfo>? bindings = null)
    {
        return new QueueInfoResult
        {
            Success = true,
            Info = info,
            Bindings = bindings
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueInfoResult CreateFailure(string errorMessage)
    {
        return new QueueInfoResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue statistics query
/// </summary>
public class QueueStatisticsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue statistics
    /// </summary>
    public QueueStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Historical data if requested
    /// </summary>
    public IEnumerable<QueueStatisticsSnapshot>? History { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueStatisticsResult CreateSuccess(QueueStatistics statistics, IEnumerable<QueueStatisticsSnapshot>? history = null)
    {
        return new QueueStatisticsResult
        {
            Success = true,
            Statistics = statistics,
            History = history
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueStatisticsResult CreateFailure(string errorMessage)
    {
        return new QueueStatisticsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue message count query
/// </summary>
public class QueueMessageCountResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Total message count
    /// </summary>
    public uint TotalMessages { get; set; }
    
    /// <summary>
    /// Ready message count
    /// </summary>
    public uint ReadyMessages { get; set; }
    
    /// <summary>
    /// Unacknowledged message count
    /// </summary>
    public uint UnacknowledgedMessages { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueMessageCountResult CreateSuccess(uint totalMessages, uint readyMessages, uint unacknowledgedMessages)
    {
        return new QueueMessageCountResult
        {
            Success = true,
            TotalMessages = totalMessages,
            ReadyMessages = readyMessages,
            UnacknowledgedMessages = unacknowledgedMessages
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueMessageCountResult CreateFailure(string errorMessage)
    {
        return new QueueMessageCountResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue consumer count query
/// </summary>
public class QueueConsumerCountResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Consumer count
    /// </summary>
    public uint ConsumerCount { get; set; }
    
    /// <summary>
    /// Consumer details if requested
    /// </summary>
    public IEnumerable<QueueConsumerInfo>? ConsumerDetails { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueConsumerCountResult CreateSuccess(uint consumerCount, IEnumerable<QueueConsumerInfo>? consumerDetails = null)
    {
        return new QueueConsumerCountResult
        {
            Success = true,
            ConsumerCount = consumerCount,
            ConsumerDetails = consumerDetails
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueConsumerCountResult CreateFailure(string errorMessage)
    {
        return new QueueConsumerCountResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue exists query
/// </summary>
public class QueueExistsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Whether the queue exists
    /// </summary>
    public bool Exists { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueExistsResult CreateSuccess(bool exists)
    {
        return new QueueExistsResult
        {
            Success = true,
            Exists = exists
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueExistsResult CreateFailure(string errorMessage)
    {
        return new QueueExistsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue bindings query
/// </summary>
public class QueueBindingsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue bindings
    /// </summary>
    public IEnumerable<QueueBindingInfo> Bindings { get; set; } = Enumerable.Empty<QueueBindingInfo>();
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueBindingsResult CreateSuccess(IEnumerable<QueueBindingInfo> bindings)
    {
        return new QueueBindingsResult
        {
            Success = true,
            Bindings = bindings
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueBindingsResult CreateFailure(string errorMessage)
    {
        return new QueueBindingsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue health query
/// </summary>
public class QueueHealthResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Overall health status
    /// </summary>
    public QueueHealthStatus HealthStatus { get; set; }
    
    /// <summary>
    /// Individual queue health information
    /// </summary>
    public IEnumerable<QueueHealthInfo>? QueueHealthInfo { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueHealthResult CreateSuccess(QueueHealthStatus healthStatus, IEnumerable<QueueHealthInfo>? queueHealthInfo = null)
    {
        return new QueueHealthResult
        {
            Success = true,
            HealthStatus = healthStatus,
            QueueHealthInfo = queueHealthInfo
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueHealthResult CreateFailure(string errorMessage)
    {
        return new QueueHealthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of queue performance query
/// </summary>
public class QueuePerformanceResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Performance data
    /// </summary>
    public QueuePerformanceData? PerformanceData { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueuePerformanceResult CreateSuccess(QueuePerformanceData performanceData)
    {
        return new QueuePerformanceResult
        {
            Success = true,
            PerformanceData = performanceData
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueuePerformanceResult CreateFailure(string errorMessage)
    {
        return new QueuePerformanceResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of list queues query
/// </summary>
public class QueueListResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Queue list
    /// </summary>
    public IEnumerable<QueueInfo> Queues { get; set; } = Enumerable.Empty<QueueInfo>();
    
    /// <summary>
    /// Total number of queues (before filtering)
    /// </summary>
    public int TotalQueues { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Query timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static QueueListResult CreateSuccess(IEnumerable<QueueInfo> queues, int totalQueues)
    {
        return new QueueListResult
        {
            Success = true,
            Queues = queues,
            TotalQueues = totalQueues
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static QueueListResult CreateFailure(string errorMessage)
    {
        return new QueueListResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

// Supporting data models

/// <summary>
/// Queue statistics snapshot
/// </summary>
public class QueueStatisticsSnapshot
{
    /// <summary>
    /// Snapshot timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// Statistics at this point in time
    /// </summary>
    public QueueStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Queue consumer information
/// </summary>
public class QueueConsumerInfo
{
    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer channel
    /// </summary>
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Consumer connection
    /// </summary>
    public string Connection { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether consumer acknowledges messages
    /// </summary>
    public bool Acknowledges { get; set; }
    
    /// <summary>
    /// Consumer exclusive flag
    /// </summary>
    public bool Exclusive { get; set; }
    
    /// <summary>
    /// Consumer prefetch count
    /// </summary>
    public ushort PrefetchCount { get; set; }
}

/// <summary>
/// Queue health status
/// </summary>
public enum QueueHealthStatus
{
    /// <summary>
    /// All queues are healthy
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Some queues have warnings
    /// </summary>
    Warning,
    
    /// <summary>
    /// Some queues are unhealthy
    /// </summary>
    Unhealthy,
    
    /// <summary>
    /// Critical queue issues
    /// </summary>
    Critical
}

/// <summary>
/// Queue health information
/// </summary>
public class QueueHealthInfo
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Health status
    /// </summary>
    public QueueHealthStatus Status { get; set; }
    
    /// <summary>
    /// Health messages
    /// </summary>
    public List<string> Messages { get; set; } = new();
    
    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTimeOffset LastCheckTimestamp { get; set; }
}

/// <summary>
/// Queue performance data
/// </summary>
public class QueuePerformanceData
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Time window for the data
    /// </summary>
    public TimeSpan TimeWindow { get; set; }
    
    /// <summary>
    /// Message throughput (messages per second)
    /// </summary>
    public double MessageThroughput { get; set; }
    
    /// <summary>
    /// Average message latency
    /// </summary>
    public TimeSpan AverageLatency { get; set; }
    
    /// <summary>
    /// Consumer utilization percentage
    /// </summary>
    public double ConsumerUtilization { get; set; }
    
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// Disk usage in bytes
    /// </summary>
    public long DiskUsage { get; set; }
    
    /// <summary>
    /// Data collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
} 