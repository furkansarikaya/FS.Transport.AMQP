using FS.Mediator.Features.RequestHandling.Core;

namespace FS.RabbitMQ.Exchange;

/// <summary>
/// Query to get exchange information
/// </summary>
public class GetExchangeInfoQuery : IRequest<ExchangeInfoResult>
{
    /// <summary>
    /// Exchange name
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
    /// Creates an exchange info query
    /// </summary>
    public static GetExchangeInfoQuery Create(string name, bool includeDetails = true, bool includeBindings = true, string? correlationId = null)
    {
        return new GetExchangeInfoQuery
        {
            Name = name,
            IncludeDetails = includeDetails,
            IncludeBindings = includeBindings,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get exchange statistics
/// </summary>
public class GetExchangeStatisticsQuery : IRequest<ExchangeStatisticsResult>
{
    /// <summary>
    /// Exchange name (optional, null for all exchanges)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Statistics categories to include
    /// </summary>
    public ExchangeStatisticsCategory Categories { get; set; } = ExchangeStatisticsCategory.All;
    
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
    /// Creates an exchange statistics query
    /// </summary>
    public static GetExchangeStatisticsQuery Create(string? name = null, ExchangeStatisticsCategory categories = ExchangeStatisticsCategory.All, 
        bool includeHistory = false, TimeSpan? historyPeriod = null, string? correlationId = null)
    {
        return new GetExchangeStatisticsQuery
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
/// Query to check if exchange exists
/// </summary>
public class ExchangeExistsQuery : IRequest<ExchangeExistsResult>
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an exchange exists query
    /// </summary>
    public static ExchangeExistsQuery Create(string name, string? correlationId = null)
    {
        return new ExchangeExistsQuery
        {
            Name = name,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get exchange bindings
/// </summary>
public class GetExchangeBindingsQuery : IRequest<ExchangeBindingsResult>
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding type filter (source/destination)
    /// </summary>
    public ExchangeBindingType? BindingType { get; set; }
    
    /// <summary>
    /// Exchange name filter for bindings
    /// </summary>
    public string? ExchangeFilter { get; set; }
    
    /// <summary>
    /// Routing key filter
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
    /// Creates an exchange bindings query
    /// </summary>
    public static GetExchangeBindingsQuery Create(string name, ExchangeBindingType? bindingType = null, 
        string? exchangeFilter = null, string? routingKeyFilter = null, bool includeArguments = true, string? correlationId = null)
    {
        return new GetExchangeBindingsQuery
        {
            Name = name,
            BindingType = bindingType,
            ExchangeFilter = exchangeFilter,
            RoutingKeyFilter = routingKeyFilter,
            IncludeArguments = includeArguments,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get exchange health status
/// </summary>
public class GetExchangeHealthQuery : IRequest<ExchangeHealthResult>
{
    /// <summary>
    /// Exchange name (optional, null for all exchanges)
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
    /// Creates an exchange health query
    /// </summary>
    public static GetExchangeHealthQuery Create(string? name = null, bool includeDetails = true, TimeSpan? timeout = null, string? correlationId = null)
    {
        return new GetExchangeHealthQuery
        {
            Name = name,
            IncludeDetails = includeDetails,
            Timeout = timeout,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get exchange performance metrics
/// </summary>
public class GetExchangePerformanceQuery : IRequest<ExchangePerformanceResult>
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Time window for metrics
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Performance metrics to include
    /// </summary>
    public ExchangePerformanceMetrics Metrics { get; set; } = ExchangePerformanceMetrics.All;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an exchange performance query
    /// </summary>
    public static GetExchangePerformanceQuery Create(string name, TimeSpan? timeWindow = null, 
        ExchangePerformanceMetrics metrics = ExchangePerformanceMetrics.All, string? correlationId = null)
    {
        return new GetExchangePerformanceQuery
        {
            Name = name,
            TimeWindow = timeWindow ?? TimeSpan.FromMinutes(5),
            Metrics = metrics,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to list all exchanges
/// </summary>
public class ListExchangesQuery : IRequest<ExchangeListResult>
{
    /// <summary>
    /// Name pattern filter (optional)
    /// </summary>
    public string? NamePattern { get; set; }
    
    /// <summary>
    /// Exchange type filter (optional)
    /// </summary>
    public string? TypeFilter { get; set; }
    
    /// <summary>
    /// Whether to include only durable exchanges
    /// </summary>
    public bool DurableOnly { get; set; } = false;
    
    /// <summary>
    /// Whether to include exchange details
    /// </summary>
    public bool IncludeDetails { get; set; } = false;
    
    /// <summary>
    /// Maximum number of exchanges to return
    /// </summary>
    public int? MaxResults { get; set; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates a list exchanges query
    /// </summary>
    public static ListExchangesQuery Create(string? namePattern = null, string? typeFilter = null, bool durableOnly = false, 
        bool includeDetails = false, int? maxResults = null, string? correlationId = null)
    {
        return new ListExchangesQuery
        {
            NamePattern = namePattern,
            TypeFilter = typeFilter,
            DurableOnly = durableOnly,
            IncludeDetails = includeDetails,
            MaxResults = maxResults,
            CorrelationId = correlationId
        };
    }
}

/// <summary>
/// Query to get exchange routing topology
/// </summary>
public class GetExchangeTopologyQuery : IRequest<ExchangeTopologyResult>
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum depth for topology traversal
    /// </summary>
    public int MaxDepth { get; set; } = 3;
    
    /// <summary>
    /// Whether to include queue bindings
    /// </summary>
    public bool IncludeQueues { get; set; } = true;
    
    /// <summary>
    /// Whether to include exchange bindings
    /// </summary>
    public bool IncludeExchanges { get; set; } = true;
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Creates an exchange topology query
    /// </summary>
    public static GetExchangeTopologyQuery Create(string name, int maxDepth = 3, bool includeQueues = true, 
        bool includeExchanges = true, string? correlationId = null)
    {
        return new GetExchangeTopologyQuery
        {
            Name = name,
            MaxDepth = maxDepth,
            IncludeQueues = includeQueues,
            IncludeExchanges = includeExchanges,
            CorrelationId = correlationId
        };
    }
}

// Supporting enums

/// <summary>
/// Exchange binding type
/// </summary>
public enum ExchangeBindingType
{
    /// <summary>
    /// Exchange is the source (bindings from this exchange)
    /// </summary>
    Source,
    
    /// <summary>
    /// Exchange is the destination (bindings to this exchange)
    /// </summary>
    Destination
}

/// <summary>
/// Exchange performance metrics
/// </summary>
[Flags]
public enum ExchangePerformanceMetrics
{
    /// <summary>
    /// Message throughput
    /// </summary>
    Throughput = 1,
    
    /// <summary>
    /// Message routing latency
    /// </summary>
    RoutingLatency = 2,
    
    /// <summary>
    /// Binding utilization
    /// </summary>
    BindingUtilization = 4,
    
    /// <summary>
    /// Memory usage
    /// </summary>
    MemoryUsage = 8,
    
    /// <summary>
    /// CPU usage
    /// </summary>
    CpuUsage = 16,
    
    /// <summary>
    /// All performance metrics
    /// </summary>
    All = Throughput | RoutingLatency | BindingUtilization | MemoryUsage | CpuUsage
}

// Result models

/// <summary>
/// Result of exchange info query
/// </summary>
public class ExchangeInfoResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange information
    /// </summary>
    public ExchangeInfo? Info { get; set; }
    
    /// <summary>
    /// Exchange bindings if requested
    /// </summary>
    public IEnumerable<ExchangeBindingInfo>? Bindings { get; set; }
    
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
    public static ExchangeInfoResult CreateSuccess(ExchangeInfo info, IEnumerable<ExchangeBindingInfo>? bindings = null)
    {
        return new ExchangeInfoResult
        {
            Success = true,
            Info = info,
            Bindings = bindings
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeInfoResult CreateFailure(string errorMessage)
    {
        return new ExchangeInfoResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange statistics query
/// </summary>
public class ExchangeStatisticsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange statistics
    /// </summary>
    public ExchangeStatistics? Statistics { get; set; }
    
    /// <summary>
    /// Historical data if requested
    /// </summary>
    public IEnumerable<ExchangeStatisticsSnapshot>? History { get; set; }
    
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
    public static ExchangeStatisticsResult CreateSuccess(ExchangeStatistics statistics, IEnumerable<ExchangeStatisticsSnapshot>? history = null)
    {
        return new ExchangeStatisticsResult
        {
            Success = true,
            Statistics = statistics,
            History = history
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeStatisticsResult CreateFailure(string errorMessage)
    {
        return new ExchangeStatisticsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange exists query
/// </summary>
public class ExchangeExistsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Whether the exchange exists
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
    public static ExchangeExistsResult CreateSuccess(bool exists)
    {
        return new ExchangeExistsResult
        {
            Success = true,
            Exists = exists
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeExistsResult CreateFailure(string errorMessage)
    {
        return new ExchangeExistsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange bindings query
/// </summary>
public class ExchangeBindingsResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange bindings
    /// </summary>
    public IEnumerable<ExchangeBindingInfo> Bindings { get; set; } = Enumerable.Empty<ExchangeBindingInfo>();
    
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
    public static ExchangeBindingsResult CreateSuccess(IEnumerable<ExchangeBindingInfo> bindings)
    {
        return new ExchangeBindingsResult
        {
            Success = true,
            Bindings = bindings
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeBindingsResult CreateFailure(string errorMessage)
    {
        return new ExchangeBindingsResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange health query
/// </summary>
public class ExchangeHealthResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Overall health status
    /// </summary>
    public ExchangeHealthStatus HealthStatus { get; set; }
    
    /// <summary>
    /// Individual exchange health information
    /// </summary>
    public IEnumerable<ExchangeHealthInfo>? ExchangeHealthInfo { get; set; }
    
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
    public static ExchangeHealthResult CreateSuccess(ExchangeHealthStatus healthStatus, IEnumerable<ExchangeHealthInfo>? exchangeHealthInfo = null)
    {
        return new ExchangeHealthResult
        {
            Success = true,
            HealthStatus = healthStatus,
            ExchangeHealthInfo = exchangeHealthInfo
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeHealthResult CreateFailure(string errorMessage)
    {
        return new ExchangeHealthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange performance query
/// </summary>
public class ExchangePerformanceResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Performance data
    /// </summary>
    public ExchangePerformanceData? PerformanceData { get; set; }
    
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
    public static ExchangePerformanceResult CreateSuccess(ExchangePerformanceData performanceData)
    {
        return new ExchangePerformanceResult
        {
            Success = true,
            PerformanceData = performanceData
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangePerformanceResult CreateFailure(string errorMessage)
    {
        return new ExchangePerformanceResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of list exchanges query
/// </summary>
public class ExchangeListResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange list
    /// </summary>
    public IEnumerable<ExchangeInfo> Exchanges { get; set; } = Enumerable.Empty<ExchangeInfo>();
    
    /// <summary>
    /// Total number of exchanges (before filtering)
    /// </summary>
    public int TotalExchanges { get; set; }
    
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
    public static ExchangeListResult CreateSuccess(IEnumerable<ExchangeInfo> exchanges, int totalExchanges)
    {
        return new ExchangeListResult
        {
            Success = true,
            Exchanges = exchanges,
            TotalExchanges = totalExchanges
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeListResult CreateFailure(string errorMessage)
    {
        return new ExchangeListResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of exchange topology query
/// </summary>
public class ExchangeTopologyResult
{
    /// <summary>
    /// Whether the query was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Exchange topology
    /// </summary>
    public ExchangeTopology? Topology { get; set; }
    
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
    public static ExchangeTopologyResult CreateSuccess(ExchangeTopology topology)
    {
        return new ExchangeTopologyResult
        {
            Success = true,
            Topology = topology
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static ExchangeTopologyResult CreateFailure(string errorMessage)
    {
        return new ExchangeTopologyResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

// Supporting data models

/// <summary>
/// Exchange statistics snapshot
/// </summary>
public class ExchangeStatisticsSnapshot
{
    /// <summary>
    /// Snapshot timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// Statistics at this point in time
    /// </summary>
    public ExchangeStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Exchange health status
/// </summary>
public enum ExchangeHealthStatus
{
    /// <summary>
    /// All exchanges are healthy
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Some exchanges have warnings
    /// </summary>
    Warning,
    
    /// <summary>
    /// Some exchanges are unhealthy
    /// </summary>
    Unhealthy,
    
    /// <summary>
    /// Critical exchange issues
    /// </summary>
    Critical
}

/// <summary>
/// Exchange health information
/// </summary>
public class ExchangeHealthInfo
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Health status
    /// </summary>
    public ExchangeHealthStatus Status { get; set; }
    
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
/// Exchange performance data
/// </summary>
public class ExchangePerformanceData
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Time window for the data
    /// </summary>
    public TimeSpan TimeWindow { get; set; }
    
    /// <summary>
    /// Message throughput (messages per second)
    /// </summary>
    public double MessageThroughput { get; set; }
    
    /// <summary>
    /// Average routing latency
    /// </summary>
    public TimeSpan AverageRoutingLatency { get; set; }
    
    /// <summary>
    /// Binding utilization percentage
    /// </summary>
    public double BindingUtilization { get; set; }
    
    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsage { get; set; }
    
    /// <summary>
    /// Data collection timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Exchange topology information
/// </summary>
public class ExchangeTopology
{
    /// <summary>
    /// Root exchange name
    /// </summary>
    public string RootExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange nodes in the topology
    /// </summary>
    public IEnumerable<ExchangeTopologyNode> ExchangeNodes { get; set; } = Enumerable.Empty<ExchangeTopologyNode>();
    
    /// <summary>
    /// Queue nodes in the topology
    /// </summary>
    public IEnumerable<QueueTopologyNode> QueueNodes { get; set; } = Enumerable.Empty<QueueTopologyNode>();
    
    /// <summary>
    /// Bindings in the topology
    /// </summary>
    public IEnumerable<TopologyBinding> Bindings { get; set; } = Enumerable.Empty<TopologyBinding>();
    
    /// <summary>
    /// Topology depth
    /// </summary>
    public int Depth { get; set; }
    
    /// <summary>
    /// Topology generation timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Exchange topology node
/// </summary>
public class ExchangeTopologyNode
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Exchange type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Node depth in topology
    /// </summary>
    public int Depth { get; set; }
    
    /// <summary>
    /// Whether exchange is durable
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Whether exchange is internal
    /// </summary>
    public bool Internal { get; set; }
}

/// <summary>
/// Queue topology node
/// </summary>
public class QueueTopologyNode
{
    /// <summary>
    /// Queue name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Node depth in topology
    /// </summary>
    public int Depth { get; set; }
    
    /// <summary>
    /// Whether queue is durable
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Message count in queue
    /// </summary>
    public uint MessageCount { get; set; }
    
    /// <summary>
    /// Consumer count
    /// </summary>
    public uint ConsumerCount { get; set; }
}

/// <summary>
/// Topology binding
/// </summary>
public class TopologyBinding
{
    /// <summary>
    /// Source name (exchange or queue)
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination name (exchange or queue)
    /// </summary>
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding type
    /// </summary>
    public TopologyBindingType Type { get; set; }
    
    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Binding arguments
    /// </summary>
    public IDictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// Topology binding type
/// </summary>
public enum TopologyBindingType
{
    /// <summary>
    /// Exchange to exchange binding
    /// </summary>
    ExchangeToExchange,
    
    /// <summary>
    /// Exchange to queue binding
    /// </summary>
    ExchangeToQueue
} 